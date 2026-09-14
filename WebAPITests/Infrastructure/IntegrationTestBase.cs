// WebAPITests/Infrastructure/IntegrationTestBase.cs
using Application.DTOs.User;
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

        _respawner = await Respawner.CreateAsync(db.Database.GetDbConnection(), new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });
    }

    public async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await _respawner.ResetAsync(db.Database.GetDbConnection());
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
        var adminPassword = "Admin123!@#";

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = adminEmail,
            Password = adminPassword
        });

        loginResponse.EnsureSuccessStatusCode();
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return auth!.Token;
    }

    // ✅ Хелпер: подтверждение email через HTTP-эндпоинт
    protected async Task ConfirmEmailViaApiAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.FirstAsync(u => u.Email == email);
        var token = user.EmailConfirmationToken
            ?? throw new InvalidOperationException(
                $"User {email} has no confirmation token. " +
                "Ensure RegisterCommandHandler generates it.");

        var response = await Client.GetAsync(
            $"/api/auth/confirm-email?userId={user.Id}&token={token}");

        response.EnsureSuccessStatusCode();
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
        var result = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
        return result!.OrderId;
    }
}

public record CreateOrderResponse(int OrderId);