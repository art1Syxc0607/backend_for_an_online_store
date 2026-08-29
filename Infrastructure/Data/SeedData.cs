// Infrastructure/Data/SeedData.cs
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // ===== 1. АДМИН =====
        if (!await context.Users.AnyAsync(u => u.Email == "admin@store.com"))
        {
            var admin = new User(
                "admin@store.com",
                passwordHasher.HashPassword("Admin123!"),
                "Admin"
            );
            admin.PromoteToAdmin();

            // Генерируем токен и сразу подтверждаем
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            admin.GenerateEmailConfirmationToken(token, DateTime.UtcNow.AddHours(24));
            admin.ConfirmEmail(token); // ← теперь токен валидный!

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }

        //if (!await context.Users.AnyAsync(u => u.Email == "admin@store.com"))
        //{

        //}

            // ===== 2. КАТЕГОРИИ =====
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new Category("Электроника", "Смартфоны, ноутбуки, планшеты"),
                new Category("Одежда", "Мужская и женская одежда"),
                new Category("Книги", "Художественная и техническая литература"),
                new Category("Дом и сад", "Мебель, инструменты, растения")
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        // ===== 3. ПРОДУКТЫ =====
        if (!await context.Products.AnyAsync())
        {
            var categories = await context.Categories.ToListAsync();
            var electronics = categories.First(c => c.Name == "Электроника");
            var clothing = categories.First(c => c.Name == "Одежда");
            var books = categories.First(c => c.Name == "Книги");

            var products = new List<Product>
            {
                new Product("iPhone 15 Pro", 999.99m, 750.00m, 10, "Последний iPhone", electronics.Id),
                new Product("Samsung Galaxy S24", 899.99m, 650.00m, 15, "Флагман Samsung", electronics.Id),
                new Product("MacBook Pro 14\"", 1999.99m, 1400.00m, 5, "Профессиональный ноутбук", electronics.Id),
                new Product("Футболка хлопковая", 29.99m, 15.00m, 50, "Качественная футболка", clothing.Id),
                new Product("Джинсы классические", 79.99m, 40.00m, 30, "Синие джинсы", clothing.Id),
                new Product("Clean Architecture", 49.99m, 25.00m, 20, "Книга Роберта Мартина", books.Id),
                new Product("C# 12 и .NET 8", 64.99m, 35.00m, 15, "Современный C#", books.Id)
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        // ===== 4. ДОБАВИТЬ КОРЗИНЫ ДЛЯ ПОЛЬЗОВАТЕЛЕЙ (опционально) =====
        // ...
    }
}