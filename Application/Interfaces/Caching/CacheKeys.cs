using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Caching;

public static class CacheKeys
{
    // ═══════════════════════════════════════════
    // Префиксы (для инвалидации)
    // ═══════════════════════════════════════════

    public const string ProductsPrefix = "products:";
    public const string PopularProductsPrefix = "products:popular:";
    public const string CategoriesPrefix = "categories:";
    public const string CartPrefix = "cart:";
    public const string ReviewsPrefix = "reviews:";
    public const string UsersPrefix = "users:";

    // ═══════════════════════════════════════════
    // Методы формирования ключей
    // ═══════════════════════════════════════════

    public static string Product(int id)
        => $"{ProductsPrefix}{id}";

    public static string ProductsList(int page, int size)
        => $"{ProductsPrefix}list:p{page}_s{size}";

    public static string PopularProducts(
        DateTime from, DateTime to, int page, int size)
        => $"{PopularProductsPrefix}{from:yyyyMMdd}_{to:yyyyMMdd}_p{page}_s{size}";

    public static string Categories()
        => $"{CategoriesPrefix}all";

    public static string Cart(int userId)
        => $"{CartPrefix}{userId}";

    public static string ReviewsForProduct(int productId)
        => $"{ReviewsPrefix}product:{productId}";
}
