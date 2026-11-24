// WebAPITests/Infrastructure/IntegrationTestBase.cs
using Application.DTOs.User;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Respawn.Graph;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace WebAPITests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    private Respawner _respawner = null!;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });

        // Каждый тест стартует с чистой БД (после ResetAsync в DisposeAsync),
        // поэтому восстанавливаем минимальные seed-данные (админ, базовые категории).
        // 1000 товаров из ProductSeeder не нужны для тестов и замедляют прогон.
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await SeedData.InitializeAsync(db, passwordHasher);
    }

    public async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    // ✅ Хелпер: регистрация + подтверждение email + логин
    protected async Task<string> RegisterAndLoginAsync(
        string email = "test@mail.com",
        string password = "Test123!@#",
        string userName = "TestUser")
    {
        // 1. Регистрация
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Password = password,
            UserName = userName
        });
        registerResponse.EnsureSuccessStatusCode();

        // 2. ✅ Подтверждение email через реальный HTTP-эндпоинт
        await ConfirmEmailViaApiAsync(email);

        // 3. Логин
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    // ✅ Хелпер: логин админа
    protected async Task<string> LoginAsAdminAsync()
    {
        var adminEmail = "admin@store.com";
        // SeedData хеширует именно "Admin123!" — пароль должен совпадать.
        var adminPassword = "Admin123!";

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = adminEmail,
            Password = adminPassword
        });

        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    // ✅ Хелпер: подтверждение email (напрямую в БД)
    // Эндпоинт /api/auth/confirm-email защищён [Authorize], поэтому в тестах
    // подтверждаем через доменную логику, используя выданный при регистрации токен.
    protected async Task ConfirmEmailViaApiAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.FirstAsync(u => u.Email == email);
        var token = user.EmailConfirmationToken
            ?? throw new InvalidOperationException(
                $"User {email} has no confirmation token. " +
                "Ensure RegisterCommandHandler generates it.");

        user.ConfirmEmail(token);
        await db.SaveChangesAsync();
    }

    // ✅ Хелпер: установка JWT
    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // ✅ Хелпер: создание продукта
    protected async Task<int> CreateProductAsync(
        string name = "Test Product",
        decimal price = 100m,
        int stock = 10)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var product = new Domain.Entities.Product(
            name,
            price,
            price * 0.7m,
            stock,
            "Test description");

        await db.Products.AddAsync(product);
        await db.SaveChangesAsync();
        return product.Id;
    }

    // ✅ Хелпер: создание заказа
    protected async Task<int> CreateOrderAsync(int productId, int quantity = 1)
    {
        var response = await Client.PostAsJsonAsync("/api/order", new
        {
            ShippingAddress = "ул. Ленина, д. 1",
            Items = new[] { new { ProductId = productId, Quantity = quantity } }
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<int>();
        return result;
    }
}