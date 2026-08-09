using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);

        // ✅ Индекс для быстрой группировки по ProductId
        builder.HasIndex(oi => oi.ProductId)
            .HasDatabaseName("IX_OrderItems_ProductId");

        // ✅ Индекс для фильтрации по CreatedAt (через Order)
        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);
    }
}

//Важно: индексы замедляют INSERT/UPDATE
//Каждый индекс требует обновления при добавлении/обновлении/удалении записей.

//Баланс:

//Чтение (SELECT) — индексы ускоряют

//Запись (INSERT/UPDATE) — индексы замедляют

//Вывод: Если у тебя много запросов на чтение и мало на запись, индекс нужен.
//Если магазин принимает 1000 заказов в секунду — индекс может быть нагрузкой.
