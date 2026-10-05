using Application.Interfaces;
using Application.Interfaces.Agent;
using Infrastructure.Cleanup;
using Infrastructure.Data;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Repositories.Agent;
using Infrastructure.Services;
using Infrastructure.Services.Agent;
using Infrastructure.Services.IpInfo;
using Infrastructure.Services.Payment;
using Infrastructure.Services.Payment.Strategies;
using Infrastructure.UnitOfWork;
using IPinfo;
using IPinfo.Apis;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Net.Http;


namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection Extension(this IServiceCollection services, IConfiguration configuration)
    {
        var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(password))
        {
            connectionString += $";Password={password}";
        }
        else
        {
            throw new InvalidOperationException(
    "Password not found. Set DB_PASSWORD environment variable.");
        }

        //services.AddDbContext<AppDbContext>(options => // SQLite
        //    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<AppDbContext>(options => // PostgreSQL
        options.UseNpgsql(connectionString,
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // for current reauest
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentRequestService, CurrentRequestService>();

        
        services.AddScoped<ILocationService, IpInfoLocationService>();
        services.AddScoped<IDeviceInfoService, DeviceInfoService>();

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IUnitOfWork, Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();


        //service
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // cash
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // EmailToken generator
        services.AddScoped<ITokenGenerator, TokenGenerator>();

        // Payment
        // Strategy Pattern
        // ✅ Регистрируем стратегии
        services.AddScoped<IPaymentStrategy, CardPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, GooglePayPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, ApplePayPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, SBPPaymentStrategy>();

        // ✅ Регистрируем фабрику и сервис
        services.AddScoped<IPaymentStrategyFactory, PaymentStrategyFactory>();
        services.AddScoped<IPaymentService, PaymentService>();


        // 1
        // Регистрация
        services.AddScoped<IEmailService, EmailService>();
        services.ConfigureOptions<SmtpSettingsConfigureOptions>();


        // фоновые сервисы for email
        // 1. Очередь - Singleton
        services.AddSingleton<IEmailBackgroundTaskQueue, EmailBackgroundTaskQueue>();

        // 2. Сервис - Singleton
        services.AddSingleton<EmailBackgroundService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // 3. Интерфейс для инъекции в handlers
        services.AddSingleton<IEmailBackgroundService>(provider =>
            provider.GetRequiredService<EmailBackgroundService>());

        // 4. Регистрация как HostedService
        services.AddHostedService(provider =>
            provider.GetRequiredService<EmailBackgroundService>());


        // ✅ Background Cleanup tasks
        services.AddScoped<ICleanupTask, PasswordResetCodesCleanupTask>();
        services.AddScoped<ICleanupTask, ExpiredOrdersCleanupTask>();

        // ✅ Планировщик
        services.AddHostedService<CleanupBackgroundService>();


        services.ConfigureOptions<IpInfoOptionsConfigureOptions>();

        // ✅ HttpClientFactory с настройками
        // Location
        services.AddHttpClient<ILocationService, IpInfoLocationService>(
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<IpInfoOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "ArtWebStore/1.0");
            });

        // for service deleting orders
        services.Configure<ExpiredOrdersOptions>(
            configuration.GetSection(ExpiredOrdersOptions.SectionName));


        // AI, Agent
        // 1. Регистрируем IChatClient (один раз, как Singleton)
        var groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "gsk_...";
        // Проверяем наличие ключа и выбрасываем понятное исключение, если его нет
        if (string.IsNullOrWhiteSpace(groqApiKey))
        {
            throw new InvalidOperationException(
                "Переменная окружения GROQ_API_KEY не установлена. " +
                "Задайте её командой: $env:GROQ_API_KEY='ваш_ключ'");
        }
        services.AddSingleton<IChatClient>(sp =>
            new OpenAIClient(
                new ApiKeyCredential(groqApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.groq.com/openai/v1") // ← Адрес API Groq
                })
                .GetChatClient("openai/gpt-oss-120b")
                .AsIChatClient()
        );

        // ✅ ShopTools — Singleton (безопасно!)
        services.AddSingleton<ShopTools>();

        //services.AddSingleton<AgentSessionStore, MyAgentSessionStore>();

        // 3. Регистрируем AIAgent (как Singleton — это шаблон)
        services.AddSingleton<AIAgent>(sp =>
        {
            var chatClient = sp.GetRequiredService<IChatClient>();
            var shopTools = sp.GetRequiredService<ShopTools>(); // ← Получаем экземпляр

            // Правильно: создавать инструменты внутри агента через фабрику, 
            // либо использовать IServiceScopeFactory
            return chatClient.AsAIAgent(
                instructions: "Ты — полезный ассистент в интернет магазине.",
                tools: [
                    AIFunctionFactory.Create(shopTools.GetAllProductsAsync),
                    AIFunctionFactory.Create(shopTools.SearchProductsAsync),
                    AIFunctionFactory.Create(shopTools.GetProductByIdAsync),
                    AIFunctionFactory.Create(shopTools.GetCategoriesAsync)
                ]
            );
        });

        services.AddScoped<AgentService>();

        services.AddScoped<IAgentMessageRepository, AgentMessageRepository>();
        services.AddScoped<IAgentConversationRepository, AgentConversationRepository>();
        services.AddScoped<AgentService>();

        // ✅ Cleanup сервис
        services.AddHostedService<ExpiredAgentConversationsCleanupService>();
        services.Configure<AgentOptions>(
            configuration.GetSection(AgentOptions.SectionName));



        return services;
    }
}
