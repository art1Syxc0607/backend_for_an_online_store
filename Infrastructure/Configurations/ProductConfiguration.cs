using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(70);

        builder.Property(p => p.Description).HasMaxLength(500);

        builder.Property(p => p.Price).IsRequired();

        builder.Property(p => p.StockQuantity).IsRequired();
        builder.Property(p => p.ReservedQuantity).IsRequired();
        builder.Property(p => p.AvailableQuantity).IsRequired();

        builder.Property(p => p.Sku).HasMaxLength(100);
        builder.Property(p => p.ImageUrl).HasMaxLength(2048);


        // связи

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p  => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.SetNull);



    }
}
