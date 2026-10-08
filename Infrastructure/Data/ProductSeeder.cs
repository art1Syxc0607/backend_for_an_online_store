using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Seed;

public static class ProductSeeder
{
    private static readonly string[] ProductNames =
    {
        "iPhone 15 Pro", "Samsung Galaxy S24", "Xiaomi 14", "Google Pixel 8",
        "MacBook Pro 14\"", "MacBook Air M3", "Dell XPS 13", "Lenovo ThinkPad X1",
        "ASUS ROG Zephyrus", "HP Spectre x360", "Acer Swift 5", "MSI Prestige 14",
        "Sony WH-1000XM5", "Bose QuietComfort 45", "AirPods Pro 2", "Samsung Galaxy Buds 3",
        "iPad Pro 12.9\"", "iPad Air M2", "Samsung Galaxy Tab S9", "Microsoft Surface Pro 10",
        "Apple Watch Series 10", "Samsung Galaxy Watch 7", "Garmin Fenix 7", "Fitbit Charge 6",
        "Sony PlayStation 5", "Xbox Series X", "Nintendo Switch OLED", "Steam Deck",
        "Canon EOS R6", "Nikon Z6 II", "Sony A7 IV", "Fujifilm X-T5",
        "DJI Mini 4 Pro", "GoPro Hero 12", "Insta360 X4", "DJI Osmo Pocket 3",
        "Logitech MX Master 3S", "Razer DeathAdder V3", "Corsair K100 RGB", "Keychron Q1 Pro",
        "Samsung 990 Pro 2TB", "WD Black SN850X", "Crucial P3 Plus", "Kingston Fury Renegade",
        "LG UltraGear 27\"", "Dell UltraSharp 27\"", "Samsung Odyssey G9", "ASUS ProArt 32\"",
        "HyperX Cloud III", "SteelSeries Arctis Nova Pro", "Razer BlackShark V2", "Logitech G Pro X",
        "Anker PowerCore 20000", "Belkin BoostCharge", "UGREEN Nexode 100W", "Baseus 65W GaN",
        "TP-Link Archer AX6000", "ASUS RT-AX88U", "Netgear Nighthawk RAX200", "Xiaomi AX9000",
        "Synology DS923+", "QNAP TS-464", "WD My Cloud Pro", "Asustor Lockerstor",
        "Intel Core i9-14900K", "AMD Ryzen 9 7950X", "NVIDIA RTX 4090", "AMD Radeon RX 7900 XTX"
    };

    private static readonly string[] Categories = new[]
    {
        "Смартфоны", "Ноутбуки", "Наушники", "Планшеты",
        "Умные часы", "Игровые консоли", "Фотоаппараты", "Дроны",
        "Аксессуары", "Периферия", "SSD и HDD", "Мониторы",
        "Гарнитуры", "Powerbank", "Роутеры", "NAS",
        "Комплектующие", "Видеокарты"
    };

    private static readonly string[] Descriptions = new[]
    {
        "Отличное качество и современный дизайн",
        "Мощный и надёжный, идеально для работы",
        "Лучшее соотношение цены и качества",
        "Стильный и функциональный выбор",
        "Премиум-класс для требовательных пользователей",
        "Инновационные технологии и удобство",
        "Идеально подходит для повседневного использования",
        "Высокая производительность и надёжность",
        "Компактный и удобный в использовании",
        "Профессиональное решение для задач любой сложности"
    };

    private static readonly string[] Brands = new[]
    {
        "Apple", "Samsung", "Xiaomi", "Google", "Sony", "Bose",
        "Dell", "HP", "Lenovo", "ASUS", "Acer", "MSI",
        "Logitech", "Razer", "Corsair", "HyperX", "SteelSeries"
    };

    /// <summary>
    /// Добавляет 500-1000 товаров в БД.
    /// </summary>
    public static async Task SeedProductsAsync(
        AppDbContext context,
        ILogger logger,
        int productCount = 1000)
    {
        // 1. ✅ Проверяем, есть ли уже товары
        var existingCount = await context.Products.CountAsync();
        if (existingCount > 100)
        {
            logger.LogInformation(
                "Products already seeded ({Count}). Skipping.",
                existingCount);
            return;
        }

        logger.LogInformation(
            "🌱 Starting product seeding. Target: {Count} products",
            productCount);

        // 2. ✅ Создаём категории
        var categories = await SeedCategoriesAsync(context, logger);

        // 3. ✅ Генерируем товары
        var random = new Random(42); // фиксированный seed для воспроизводимости
        var products = new List<Product>(productCount);

        for (int i = 0; i < productCount; i++)
        {
            var category = categories[random.Next(categories.Count)];
            var brand = Brands[random.Next(Brands.Length)];
            var productName = ProductNames[random.Next(ProductNames.Length)];
            var description = Descriptions[random.Next(Descriptions.Length)];

            // ✅ Цена от 500 до 200000
            var price = Math.Round(
                (decimal)(random.NextDouble() * 199_500 + 500), 2);

            // ✅ Закупочная цена — 70% от розничной
            var purchasePrice = Math.Round(price * 0.7m, 2);

            // ✅ Количество на складе от 0 до 999
            var stockQuantity = random.Next(0, 1000);

            // ✅ SKU (уникальный артикул)
            var sku = $"{brand[..Math.Min(3, brand.Length)].ToUpper()}-{100000 + i}";

            // ✅ Название с брендом и номером для уникальности
            var uniqueName = $"{brand} {productName} #{i + 1}";

            var product = new Product(
                name: uniqueName,
                price: price,
                purchasePrice: purchasePrice,
                stockQuantity: stockQuantity,
                description: description);

            // ✅ Устанавливаем SKU и CategoryId
            product.UpdateDetails(sku: sku);
            product.UpdateDetails(categoryId: category.Id);

            products.Add(product);
        }

        // 4. ✅ Сохраняем пачками (быстрее, чем по одному)
        const int batchSize = 100;
        for (int i = 0; i < products.Count; i += batchSize)
        {
            var batch = products.Skip(i).Take(batchSize);
            await context.Products.AddRangeAsync(batch);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "🌱 Seeded {Current}/{Total} products",
                Math.Min(i + batchSize, products.Count),
                products.Count);

            // ✅ Очищаем Change Tracker для экономии памяти
            context.ChangeTracker.Clear();
        }

        logger.LogInformation(
            "✅ Product seeding completed. Total: {Count} products",
            productCount);
    }

    /// <summary>
    /// Создаёт категории, если их нет.
    /// </summary>
    private static async Task<List<Category>> SeedCategoriesAsync(
        AppDbContext context,
        ILogger logger)
    {
        // ✅ Проверяем существующие
        var existingCategories = await context.Categories.ToListAsync();

        if (existingCategories.Count >= Categories.Length)
        {
            logger.LogInformation(
                "Categories already seeded ({Count}). Skipping.",
                existingCategories.Count);
            return existingCategories;
        }

        // ✅ Создаём новые
        var newCategories = new List<Category>();

        foreach (var name in Categories)
        {
            // Проверяем, есть ли уже такая
            if (existingCategories.Any(c => c.Name == name))
                continue;

            var category = new Category(name);
            newCategories.Add(category);
        }

        if (newCategories.Any())
        {
            await context.Categories.AddRangeAsync(newCategories);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "🌱 Seeded {Count} categories",
                newCategories.Count);
        }

        // ✅ Возвращаем все категории
        return await context.Categories.ToListAsync();
    }
}