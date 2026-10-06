using Application.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure.Services.Agent;

public class ShopTools
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ShopTools> _logger;

    public ShopTools(
        IServiceScopeFactory scopeFactory,
        ILogger<ShopTools> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // ═══════════════════════════════════════════
    // Товары
    // ═══════════════════════════════════════════

    [Description("Возвращает список всех товаров в каталоге магазина с ценами и наличием.")]
    public async Task<string> GetAllProductsAsync(int pageNumber, int pageSize, 
        CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            var pageResult = await repo.GetAllProductsAsync(pageNumber, pageSize, ct);

            _logger.LogInformation(
                "ShopTools: GetAllProductsAsync returned {Count} products",
                pageResult.Items.Count);

            if (!pageResult.Items.Any())
                return "Каталог товаров пуст.";

            var sb = new StringBuilder();
            sb.AppendLine($"Найдено товаров: {pageResult.Items.Count}");
            sb.AppendLine();

            foreach (var p in pageResult.Items)
            {
                sb.AppendLine($"- ID: {p.Id}");
                sb.AppendLine($"  Название: {p.Name}");
                sb.AppendLine($"  Цена: {p.Price:C}");
                sb.AppendLine($"  В наличии: {p.AvailableQuantity} шт.");

                if (!string.IsNullOrWhiteSpace(p.Description))
                    sb.AppendLine($"  Описание: {p.Description}");

                sb.AppendLine();
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShopTools: Failed to get all products");
            return "Не удалось получить список товаров. Попробуйте позже.";
        }
    }

    [Description("Ищет товары по названию. Возвращает найденные товары с ценами.")]
    public async Task<string> SearchProductsAsync(
     [Description("Поисковый запрос — часть названия товара, например 'iPhone' или 'Samsung'")]
        string searchText,
     CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return "Поисковый запрос не может быть пустым.";

            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            var products = await repo.GetProductsFilter(
                SearchText: searchText,
                ct: ct
            );

            _logger.LogInformation(
                "ShopTools: SearchProductsAsync('{Query}') returned {Count} products",
                searchText, products.Items.Count);

            if (!products.Items.Any())
                return $"Товары по запросу '{searchText}' не найдены.";

            var sb = new StringBuilder();
            sb.AppendLine($"Найдено товаров по запросу '{searchText}': {products.Items.Count}");
            sb.AppendLine();

            foreach (var p in products.Items)
            {
                sb.AppendLine($"- {p.Name} — {p.Price:C} (в наличии: {p.AvailableQuantity})");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ShopTools: Failed to search products with query '{Query}'", searchText);
            return "Не удалось выполнить поиск. Попробуйте позже.";
        }
    }

    [Description("Возвращает подробную информацию о товаре по его ID.")]
    public async Task<string> GetProductByIdAsync(
    [Description("ID товара (число), например 42")]
        int productId,
    CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

            var product = await repo.GetByIdAsync(productId, ct);

            if (product == null)
                return $"Товар с ID {productId} не найден.";

            var sb = new StringBuilder();
            sb.AppendLine($"Товар: {product.Name}");
            sb.AppendLine($"ID: {product.Id}");
            sb.AppendLine($"Цена: {product.Price:C}");
            sb.AppendLine($"В наличии: {product.AvailableQuantity} шт.");

            if (!string.IsNullOrWhiteSpace(product.Description))
                sb.AppendLine($"Описание: {product.Description}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ShopTools: Failed to get product {ProductId}", productId);
            return "Не удалось получить информацию о товаре. Попробуйте позже.";
        }
    }

    // ═══════════════════════════════════════════
    // Категории
    // ═══════════════════════════════════════════

    [Description("Возвращает список всех категорий товаров.")]
    public async Task<string> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();

            var categories = await repo.GetAllCategoriesAsync(ct);

            if (!categories.Any())
                return "Категории не найдены.";

            return string.Join("\n", categories.Select(c => $"- {c.Name}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShopTools: Failed to get categories");
            return "Не удалось получить список категорий.";
        }
    }

}