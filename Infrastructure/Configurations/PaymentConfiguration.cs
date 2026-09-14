using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.TransactionId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.ExternalTransactionId)
            .HasMaxLength(255);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(1000);

        // ✅ Связь с Order
        builder.HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);  // ← платёж нельзя удалить

        // ✅ Связь с User
        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);  // ← защита финансов

        // ✅ Индексы
        builder.HasIndex(p => p.TransactionId).IsUnique();
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.Status);
    }
}