using Application;
using Application.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;
using WebAPI.Middleware;
//using SQLitePCL;



//namespace WebApi;

//Batteries.Init(); // ДОБАВИТЬ!

var builder = WebApplication.CreateBuilder(args);

// Добавляем аутентификацию с указанием схемы
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // "Bearer"
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ValidateIssuerSigningKey = true,
            //ClockSkew = TimeSpan.Zero // опционально
        };
    });

// Добавляем авторизацию
builder.Services.AddAuthorization();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Настройка Swagger для JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Пример: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor(); // Как получить HttpContext в сервисе/команде?
builder.Services.Extension(builder.Configuration); // DbContext + Repositories
builder.Services.AddApplication();      // MediatR + FluentValidation

builder.Host.UseSerilog((context, config) =>
{
    // 1. Читаем конфигурацию из appsettings.json
    config.ReadFrom.Configuration(context.Configuration);

    // 3. Добавляем File с асинхронностью для ВСЕХ логов
    config.WriteTo.Async(a => a.File(
        path: "logs/all/log-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Verbose,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    ));

    // 4. Добавляем File с асинхронностью только для Error
    config.WriteTo.Async(a => a.File(
        path: "logs/errors/error-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:w3}] {Message:lj}{NewLine}{Exception}"
    ));
});

// реализация In-Memory Cache
builder.Services.AddMemoryCache();


var app = builder.Build();

// Добавляем middleware до всех остальных
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

// 2. МАГИЯ: ОДНА СТРОЧКА для логирования времени ВСЕХ запросов
app.UseSerilogRequestLogging(); // Важно: поставить ДО эндпоинтов!

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Добавляем middleware
app.UseAuthentication(); // СНАЧАЛА аутентификация
app.UseAuthorization();  // ПОТОМ авторизация

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher>();

    try
    {
        await SeedData.InitializeAsync(context, passwordHasher);
        Console.WriteLine("Данные инициализированы успешно!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка инициализации: {ex.Message}");
    }
}

app.Run();
