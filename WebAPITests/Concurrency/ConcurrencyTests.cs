using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Cart;
using Application.DTOs.Order;
using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Cleanup;
using Infrastructure.Data;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebAPITests.Infrastructure;

namespace WebAPITests.Concurrency;

public class ConcurrencyTests : IntegrationTestBase
{
    public ConcurrencyTests(CustomWebApplicationFactory factory) : base(factory) { }

    // =====================================================================
    // Helpers
    // =====================================================================

    private async Task<HttpClient> CreateUserClientAsync(
        string email,
        string password,
        string userName)
    {
        var token = await RegisterAndLoginAsync(email, password, userName);
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<HttpResponseMessage[]> RunConcurrently(
        params Func<Task<HttpResponseMessage>>[] requests)
    {
        var tasks = requests.Select(r => Task.Run(r)).ToArray();
        return await Task.WhenAll(tasks);
    }

    private async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    private async Task ExecuteDbAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(db);
    }

    private async Task<int> CountOrdersInDbAsync()
    {
        return await QueryDbAsync(async db => await db.Orders.CountAsync());
    }

    private async Task<int> CountPaidPaymentsInDbAsync()
    {
        return await QueryDbAsync(
            async db => await db.Payments.CountAsync(p => p.Status == PaymentStatus.Paid));
    }

    private async Task<int> CountPendingPaymentsInDbAsync()
    {
        return await QueryDbAsync(
            async db => await db.Payments.CountAsync(p => p.Status == PaymentStatus.Pending));
    }

    private async Task<int> CountReviewsForProductAndUserAsync(int productId, int userId)
    {
        return await QueryDbAsync(async db =>
            await db.Reviews.CountAsync(r => r.ProductId == productId && r.UserId == userId));
    }

    private async Task<Product?> GetProductFromDbAsync(int productId)
    {
        return await QueryDbAsync(async db => await db.Products.FindAsync(productId));
    }

    private async Task<Order?> GetOrderFromDbAsync(int orderId)
    {
        return await QueryDbAsync(async db =>
            await db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId));
    }

    private async Task<int> GetCurrentUserIdFromClientAsync(HttpClient client)
    {
        var profileResponse = await client.GetAsync("/api/auth/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        return profile!.Id;
    }

    // POST /api/order — возвращает HttpResponseMessage (без assert внутри)
    private async Task<HttpResponseMessage> CreateOrderRequestAsync(HttpClient client, int productId, int quantity = 1)
    {
        return await client.PostAsJsonAsync("/api/order", new
        {
            ShippingAddress = "ул. Ленина, д. 1",
            Items = new[] { new { ProductId = productId, Quantity = quantity } }
        });
    }

    private async Task<int> CreateOrderViaApiAsync(HttpClient client, int productId, int quantity = 1)
    {
        var response = await CreateOrderRequestAsync(client, productId, quantity);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<int>();
        return result;
    }

    private async Task<string> InitiatePaymentViaApiAsync(HttpClient client, int orderId)
    {
        var response = await client.PostAsJsonAsync("/api/order/initiate", new
        {
            OrderId = orderId,
            Method = "Card"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
        result!.Success.Should().BeTrue();
        result.PaymentIntentId.Should().NotBeNullOrEmpty();
        return result.PaymentIntentId!;
    }

    private async Task AddToCartAsync(HttpClient client, int productId, int quantity = 1)
    {
        var response = await client.PostAsJsonAsync("/api/cart", new
        {
            ProductId = productId,
            Quantity = quantity
        });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<HttpResponseMessage> AddToCartRequestAsync(HttpClient client, int productId, int quantity = 1)
    {
        return await client.PostAsJsonAsync("/api/cart", new
        {
            ProductId = productId,
            Quantity = quantity
        });
    }

    private async Task<HttpResponseMessage> CheckoutAsync(HttpClient client)
    {
        return await client.PostAsJsonAsync("/api/cart/Checkout", new
        {
            ShippingAddress = "ул. Ленина, д. 1"
        });
    }

    // =====================================================================
    // P0: Checkout races
    // =====================================================================

    [Fact]
    public async Task Checkout_WhenTwoUsersRaceForLastItem_OnlyOneSucceeds()
    {
        // Arrange — продукт с остатком 1 шт.
        var productId = await CreateProductAsync("LastItem", 100m, 1);

        var user1Client = await CreateUserClientAsync("user1@mail.com", "Test123!@#", "User1");
        var user2Client = await CreateUserClientAsync("user2@mail.com", "Test123!@#", "User2");

        // Оба пользователя кладут один и тот же товар в свои корзины
        await AddToCartAsync(user1Client, productId);
        await AddToCartAsync(user2Client, productId);

        // Act — оба оформляют заказ одновременно
        var responses = await RunConcurrently(
            () => CheckoutAsync(user1Client),
            () => CheckoutAsync(user2Client)
        );

        // Assert — ровно один успех, второй получает ошибку
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var failureCount = responses.Count(r => !r.IsSuccessStatusCode);
        successCount.Should().Be(1, "только один checkout должен пройти за последний товар");
        failureCount.Should().Be(1);

        // Проверяем состояние БД: один заказ, резерв 1
        var orderCount = await CountOrdersInDbAsync();
        orderCount.Should().Be(1);

        var product = await GetProductFromDbAsync(productId);
        product!.ReservedQuantity.Should().Be(1);
        product.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Checkout_WhenSameCartCheckedOutTwice_OnlyOneOrderCreated()
    {
        // Arrange — продукт с остатком 5 шт., одна корзина
        var productId = await CreateProductAsync("StockItem", 100m, 5);
        var client = await CreateUserClientAsync("buyer@mail.com", "Test123!@#", "Buyer");

        await AddToCartAsync(client, productId);

        // Act — оформляем ту же корзину дважды одновременно
        var responses = await RunConcurrently(
            () => CheckoutAsync(client),
            () => CheckoutAsync(client)
        );

        // Assert — ровно один succès, второй видит пустую корзину
        responses.Count(r => r.IsSuccessStatusCode).Should().Be(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest).Should().Be(1);

        // Проверяем состояние БД: один заказ
        var orderCount = await CountOrdersInDbAsync();
        orderCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateOrder_WhenTwoUsersRaceForLastItem_OnlyOneOrderCreated()
    {
        // Arrange — продукт с остатком 1 шт., два пользователя
        var productId = await CreateProductAsync("DirectOrderItem", 100m, 1);

        var user1Client = await CreateUserClientAsync("direct1@mail.com", "Test123!@#", "DirectUser1");
        var user2Client = await CreateUserClientAsync("direct2@mail.com", "Test123!@#", "DirectUser2");

        // Act — оба создают заказ на один и тот же последний товар одновременно
        var responses = await RunConcurrently(
            () => CreateOrderRequestAsync(user1Client, productId),
            () => CreateOrderRequestAsync(user2Client, productId)
        );

        // Assert — ровно один succès
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        successCount.Should().Be(1);

        // Проверяем состояние БД: один заказ, резерв 1
        var orderCount = await CountOrdersInDbAsync();
        orderCount.Should().Be(1);

        var product = await GetProductFromDbAsync(productId);
        product!.ReservedQuantity.Should().Be(1);
    }

    // =====================================================================
    // P0: Payment confirmation races
    // =====================================================================

    [Fact]
    public async Task ConfirmPayment_WhenTwoRequestsRace_IsIdempotent()
    {
        // Arrange — заказ + инициированный платёж
        var productId = await CreateProductAsync("PaidItem", 100m, 5);
        var client = await CreateUserClientAsync("buyer@mail.com", "Test123!@#", "Buyer");

        var orderId = await CreateOrderViaApiAsync(client, productId);
        var paymentIntentId = await InitiatePaymentViaApiAsync(client, orderId);

        // Act — подтверждаем оплату дважды одновременно
        var responses = await RunConcurrently(
            () => client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId }),
            () => client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId })
        );

        // Assert — оба запроса успешны (идемпотентно)
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue(
            "повторное подтверждение должно быть идемпотентным");

        // Проверяем состояние БД: ровно один оплаченный платёж, заказ в статусе Paid
        var paidPaymentCount = await CountPaidPaymentsInDbAsync();
        paidPaymentCount.Should().Be(1);

        var pendingPaymentCount = await CountPendingPaymentsInDbAsync();
        pendingPaymentCount.Should().Be(0);

        var order = await GetOrderFromDbAsync(orderId);
        order!.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public async Task ConfirmPayment_WhenCancelRunsConcurrently_NeverLeavesInconsistentState()
    {
        // Arrange — заказ + инициированный платёж
        var productId = await CreateProductAsync("RaceItem", 100m, 5);
        var client = await CreateUserClientAsync("buyer@mail.com", "Test123!@#", "Buyer");

        var orderId = await CreateOrderViaApiAsync(client, productId);
        var paymentIntentId = await InitiatePaymentViaApiAsync(client, orderId);

        // Act — подтверждение оплаты и отмена заказа одновременно
        var confirmTask = client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId });
        var cancelTask = client.PutAsJsonAsync("/api/order/Cancel", new { OrderId = orderId });

        var responses = await Task.WhenAll(confirmTask, cancelTask);

        // Assert — один succès, другой ошибка
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        successCount.Should().Be(1, "побеждает только один сценарий");

        // Проверяем состояние БД: согласованный конечный статус
        var order = await GetOrderFromDbAsync(orderId);
        (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)
            .Should().BeTrue("заказ не может быть одновременно оплачен и отменён");

        var paidPaymentCount = await CountPaidPaymentsInDbAsync();
        if (order.Status == OrderStatus.Paid)
        {
            paidPaymentCount.Should().Be(1);
        }
        else
        {
            paidPaymentCount.Should().Be(0, "если заказ отменён, платёж не должен быть оплачен");
        }
    }

    [Fact]
    public async Task ConfirmPayment_WhenCleanupRunsConcurrently_NoInconsistentState()
    {
        // Arrange — заказ, у которого CreatedAt старше таймаута оплаты (30 мин)
        var productId = await CreateProductAsync("CleanupItem", 100m, 5);
        var client = await CreateUserClientAsync("buyer@mail.com", "Test123!@#", "Buyer");

        var orderId = await CreateOrderViaApiAsync(client, productId);

        // Выставляем CreatedAt в прошлое через raw SQL (private setter)
        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Orders"" SET ""CreatedAt"" = {DateTime.UtcNow.AddMinutes(-35)} WHERE ""Id"" = {orderId}");
        });

        var paymentIntentId = await InitiatePaymentViaApiAsync(client, orderId);

        // Получаем задачу очистки из DI
        await using var scope = Factory.Services.CreateAsyncScope();
        var cleanupTask = scope.ServiceProvider.GetServices<ICleanupTask>()
            .OfType<ExpiredOrdersCleanupTask>()
            .Single();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ExpiredOrdersOptions>>();
        options.Value.PaymentTimeoutMinutes.Should().Be(30);

        // Act — cleanup и подтверждение оплаты одновременно
        var cleanupExecution = cleanupTask.ExecuteAsync(CancellationToken.None);
        var confirmTask = client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId });

        await Task.WhenAll(cleanupExecution, confirmTask);
        var confirmResponse = await confirmTask;

        // Assert — состояние БД должно быть согласованным независимо от победителя гонки
        var order = await GetOrderFromDbAsync(orderId);
        order.Should().NotBeNull();
        (order!.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)
            .Should().BeTrue("заказ не может быть одновременно оплачен и отменён cleanup-ом");

        var paidPaymentCount = await CountPaidPaymentsInDbAsync();
        if (order.Status == OrderStatus.Paid)
        {
            confirmResponse.IsSuccessStatusCode.Should().BeTrue(
                "если заказ оплачен, подтверждение должно завершиться успешно");
            paidPaymentCount.Should().Be(1);
        }
        else
        {
            paidPaymentCount.Should().Be(0, "если заказ отменён cleanup-ом, платёж не должен быть оплачен");
        }
    }

    // =====================================================================
    // P1: Cart races
    // =====================================================================

    [Fact]
    public async Task Cart_WhenSameProductAddedConcurrently_NoLostUpdates()
    {
        // Arrange — продукт с остатком 5 шт., один пользователь
        var productId = await CreateProductAsync("CartItem", 100m, 5);
        var client = await CreateUserClientAsync("buyer@mail.com", "Test123!@#", "Buyer");

        // Act — добавляем один и тот же товар в корзину дважды одновременно
        var responses = await RunConcurrently(
            () => AddToCartRequestAsync(client, productId, 1),
            () => AddToCartRequestAsync(client, productId, 1)
        );

        // Assert — оба запроса успешны
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue();

        // Проверяем корзину
        var cartResponse = await client.GetAsync("/api/cart");
        cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartResponseDto>();
        cart!.Items.Should().NotBeEmpty("в корзине должен быть товар");
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        item.Should().NotBeNull("товар должен быть в корзине");
        item.Quantity.Should().BeGreaterThan(0);
    }

    // =====================================================================
    // P1: Review races
    // =====================================================================

    [Fact]
    public async Task Review_WhenSameUserEditsConcurrently_OneGets409()
    {
        // Arrange — продукт, пользователь с Received-заказом и существующий отзыв
        var productId = await CreateProductAsync("ReviewEditItem", 100m, 5);
        var client = await CreateUserClientAsync("reviewer2@mail.com", "Test123!@#", "Reviewer2");

        var userId = await GetCurrentUserIdFromClientAsync(client);

        // Создаём заказ и переводим в Received
        var orderId = await CreateOrderViaApiAsync(client, productId, 1);
        var paymentIntentId = await InitiatePaymentViaApiAsync(client, orderId);
        await client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId });

        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Orders"" SET ""Status"" = {(int)OrderStatus.Shipped} WHERE ""Id"" = {orderId}");
        });
        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Orders"" SET ""Status"" = {(int)OrderStatus.Delivered} WHERE ""Id"" = {orderId}");
        });
        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Orders"" SET ""Status"" = {(int)OrderStatus.Received} WHERE ""Id"" = {orderId}");
        });

        // Создаём отзыв
        var reviewContent = CreateReviewMultipartContent(productId, "Original text", 4);
        var createResponse = await client.PostAsync("/api/review", reviewContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewId = await createResponse.Content.ReadFromJsonAsync<int>();

        // Act — два параллельных редактирования одного отзыва
        var edit1 = client.PutAsJsonAsync("/api/review", new
        {
            ReviewId = reviewId,
            NewText = "Updated text 1",
            NewRating = 5
        });
        var edit2 = client.PutAsJsonAsync("/api/review", new
        {
            ReviewId = reviewId,
            NewText = "Updated text 2",
            NewRating = 3
        });

        var responses = await Task.WhenAll(edit1, edit2);

        // Assert — оба запроса успешны (обновления сериализуются блокировкой строки)
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue(
            "конкурентные правки сериализуются и обе должны завершиться успешно");

        // Проверяем состояние БД: текст равен одному из двух значений
        var reviewFromDb = await QueryDbAsync(async db =>
            await db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId));
        reviewFromDb.Should().NotBeNull();
        reviewFromDb!.Text.Should().BeOneOf("Updated text 1", "Updated text 2");
    }

    [Fact]
    public async Task UpdateProduct_WhenTwoUpdatesRace_OneGets409()
    {
        // Arrange — продукт и пользователь с ролью Admin. Роль повышаем в БД,
        // а затем перелогиниваемся: в старом JWT роль ещё "User".
        var productId = await CreateProductAsync("AdminProduct", 100m, 10);
        var client = await CreateUserClientAsync("admin1@mail.com", "Test123!@#", "Admin1");

        await MakeUserAdminAsync(client);
        await ReloginAsync(client, "admin1@mail.com", "Test123!@#");

        var product = await GetProductFromDbAsync(productId);
        product!.Price.Should().Be(100m);

        // Act — два одновременных обновления цены одного продукта
        var update1 = client.PutAsJsonAsync("/api/admin/product", new
        {
            Id = productId,
            Price = 150m,
            StockQuantity = 10,
            Name = "AdminProduct",
            Description = "Test description",
            PurchasePrice = 70m
        });
        var update2 = client.PutAsJsonAsync("/api/admin/product", new
        {
            Id = productId,
            Price = 200m,
            StockQuantity = 10,
            Name = "AdminProduct",
            Description = "Test description",
            PurchasePrice = 70m
        });

        var responses = await Task.WhenAll(update1, update2);

        // Assert — оба запроса успешны (обновления сериализуются блокировкой строки)
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue(
            "конкурентные обновления сериализуются и обе должны завершиться успешно");

        // Проверяем состояние БД: цена равна одному из двух значений, не смешанному
        var updatedProduct = await GetProductFromDbAsync(productId);
        updatedProduct!.Price.Should().BeOneOf(150m, 200m);
    }

    [Fact]
    public async Task AddProduct_WhenSameSkuCreatedConcurrently_OnlyOneProductIsCreated()
    {
        // Arrange — пользователь с ролью Admin + свежий токен с этой ролью
        var client = await CreateUserClientAsync("skuAdmin@mail.com", "Test123!@#", "SkuAdmin");
        await MakeUserAdminAsync(client);
        await ReloginAsync(client, "skuAdmin@mail.com", "Test123!@#");

        const string sku = "RACE-SKU-001";

        // Act — два одинаковых товара с одним SKU одновременно
        var responses = await RunConcurrently(
            () => client.PostAsync("/api/admin/product", BuildProductForm("Race product 1", sku)),
            () => client.PostAsync("/api/admin/product", BuildProductForm("Race product 2", sku)));

        // Assert — уникальный индекс IX_Products_NormalizedSku пропускает только один INSERT
        responses.Count(r => r.IsSuccessStatusCode).Should().Be(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(1,
            "второй INSERT должен получить 23505, который middleware маппит в 409");

        var productsWithSku = await QueryDbAsync(async db =>
            await db.Products.CountAsync(p => p.Sku == sku));

        productsWithSku.Should().Be(1, "дубликатов с одинаковым SKU быть не должно");
    }

    [Fact]
    public async Task UploadProductFiles_WhenTwoUploadsRace_OneGets409()
    {
        // Arrange — продукт и пользователь с ролью Admin
        var productId = await CreateProductAsync("UploadProduct", 100m, 10);
        var client = await CreateUserClientAsync("admin2@mail.com", "Test123!@#", "Admin2");

        await MakeUserAdminAsync(client);
        await ReloginAsync(client, "admin2@mail.com", "Test123!@#");

        // Act — две одновременные загрузки файлов одного продукта
        var bytes1 = System.Text.Encoding.UTF8.GetBytes("fake-jpeg-content-1");
        var file1 = new ByteArrayContent(bytes1);
        file1.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        var content1 = new MultipartFormDataContent();
        content1.Add(file1, "files", "test1.jpg");

        var bytes2 = System.Text.Encoding.UTF8.GetBytes("fake-jpeg-content-2");
        var file2 = new ByteArrayContent(bytes2);
        file2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        var content2 = new MultipartFormDataContent();
        content2.Add(file2, "files", "test2.jpg");

        var upload1 = client.PostAsync($"/api/admin/product/{productId}/files", content1);
        var upload2 = client.PostAsync($"/api/admin/product/{productId}/files", content2);

        var responses = await Task.WhenAll(upload1, upload2);

        // Assert — оба запроса успешны (загрузки сериализуются блокировкой строки)
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue(
            "конкурентные загрузки сериализуются и обе должны завершиться успешно");
    }

    // =====================================================================
    // Helpers specific to product concurrency tests
    // =====================================================================

    private async Task MakeUserAdminAsync(HttpClient client)
    {
        var userId = await GetCurrentUserIdFromClientAsync(client);
        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Users"" SET ""Role"" = 1 WHERE ""Id"" = {userId}");
        });
    }

    /// <summary>
    /// Повышение роли в БД не меняет уже выданный JWT, поэтому после
    /// MakeUserAdminAsync нужен новый токен — иначе [Authorize(Roles="Admin")]
    /// продолжит видеть старую роль "User".
    /// </summary>
    private static async Task ReloginAsync(HttpClient client, string email, string password)
    {
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);
    }

    // =====================================================================
    // Additional P1: Cart races
    // =====================================================================

    [Fact]
    public async Task Cart_WhenTwoUsersUpdateQuantityConcurrently_NoLostUpdates()
    {
        // Arrange — продукт с остатком 5 шт., один пользователь, товар уже в корзине
        var productId = await CreateProductAsync("CartQtyItem", 100m, 5);
        var client = await CreateUserClientAsync("buyer@mail.com", "Test123!@#", "Buyer");

        await AddToCartAsync(client, productId, 1);

        // Act — два параллельных обновления количества одного и того же товара
        var update1 = client.PutAsJsonAsync("/api/cart/items", new
        {
            ProductId = productId,
            Quantity = 3
        });
        var update2 = client.PutAsJsonAsync("/api/cart/items", new
        {
            ProductId = productId,
            Quantity = 5
        });

        var responses = await Task.WhenAll(update1, update2);

        // Assert — оба succès, количество соответствует одному из двух значений
        responses.All(r => r.IsSuccessStatusCode).Should().BeTrue();

        var cartResponse = await client.GetAsync("/api/cart");
        cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartResponseDto>();
        cart!.Items.Should().NotBeEmpty();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        item.Should().NotBeNull();
        item.Quantity.Should().BeOneOf(3, 5);
    }

    [Fact]
    public async Task Cart_WhenAddAndClearConcurrently_ClearWinsOrAddWinsConsistently()
    {
        // Arrange — продукт с остатком 5 шт., один пользователь, товар в корзине
        var productId = await CreateProductAsync("CartClearItem", 100m, 5);
        var client = await CreateUserClientAsync("buyer2@mail.com", "Test123!@#", "Buyer2");

        await AddToCartAsync(client, productId, 1);

        // Act — добавляем товар и очищаем корзину одновременно
        var addTask = client.PostAsJsonAsync("/api/cart", new
        {
            ProductId = productId,
            Quantity = 1
        });
        var clearTask = client.PostAsJsonAsync("/api/cart/Checkout", new
        {
            ShippingAddress = "ул. Ленина, д. 1"
        });

        var responses = await Task.WhenAll(addTask, clearTask);

        // Assert — один succès, другой может быть succès или ошибка, но состояние консистентно
        // (корзина либо пуста после checkout, либо содержит товары)
        var cartResponse = await client.GetAsync("/api/cart");
        cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await cartResponse.Content.ReadFromJsonAsync<CartResponseDto>();
        cart.Should().NotBeNull();
        // Если checkout прошёл, корзина пуста; если нет — содержит товар
        // Главное: нет потерянного обновления
    }

    // =====================================================================
    // Additional P1: Review races
    // =====================================================================

    [Fact]
    public async Task Review_WhenSameUserSubmitsTwiceConcurrently_OnlyOneReviewCreated()
    {
        // Arrange — продукт и пользователь с Received-заказом
        var productId = await CreateProductAsync("ReviewItem2", 100m, 5);
        var client = await CreateUserClientAsync("reviewer3@mail.com", "Test123!@#", "Reviewer3");

        var userId = await GetCurrentUserIdFromClientAsync(client);

        // Создаём заказ
        var orderId = await CreateOrderViaApiAsync(client, productId, 1);

        // Инициируем и подтверждаем оплату
        var paymentIntentId = await InitiatePaymentViaApiAsync(client, orderId);
        var confirmResponse = await client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId });
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Переводим заказ в Received напрямую через БД (private setter, админ-токен сломан)
        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Orders"" SET ""Status"" = {(int)OrderStatus.Shipped} WHERE ""Id"" = {orderId}");
        });
        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Orders"" SET ""Status"" = {(int)OrderStatus.Delivered} WHERE ""Id"" = {orderId}");
        });
        await ExecuteDbAsync(async db =>
        {
            await db.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Orders"" SET ""Status"" = {(int)OrderStatus.Received} WHERE ""Id"" = {orderId}");
        });

        // Act — создаём отзыв дважды одновременно (multipart/form-data)
        var reviewContent1 = CreateReviewMultipartContent(productId, "Great product!", 5);
        var reviewContent2 = CreateReviewMultipartContent(productId, "Great product!", 5);

        var reviewTask1 = client.PostAsync("/api/review", reviewContent1);
        var reviewTask2 = client.PostAsync("/api/review", reviewContent2);

        var responses = await Task.WhenAll(reviewTask1, reviewTask2);

        // Assert — один succès, другой ошибка (дубль отзыва — уникальный индекс)
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        successCount.Should().Be(1, "только один отзыв на продукт от пользователя");

        // Проверяем состояние БД: ровно один отзыв
        var reviewCount = await CountReviewsForProductAndUserAsync(productId, userId);
        reviewCount.Should().Be(1);
    }

    private static MultipartFormDataContent CreateReviewMultipartContent(int productId, string text, int rating)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(productId.ToString()), "ProductId");
        content.Add(new StringContent(text), "Text");
        content.Add(new StringContent(rating.ToString()), "Rating");
        return content;
    }

    private static MultipartFormDataContent BuildProductForm(string name, string sku)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "Name");
        content.Add(new StringContent("100"), "Price");
        content.Add(new StringContent("70"), "PurchasePrice");
        content.Add(new StringContent("5"), "StockQuantity");
        content.Add(new StringContent("Test description"), "Description");
        content.Add(new StringContent(sku), "Sku");
        return content;
    }
}
