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
        builder.HasIndex(p => p.Status);

        // An order must not have more than one active (Pending) payment at a time.
        // This is the database-level guarantee behind InitiatePaymentHandler.
        // EF keeps a single index per property set, so this partial unique index
        // also replaces the former plain IX_Payments_OrderId.
        builder.HasIndex(p => p.OrderId)
            .HasDatabaseName("IX_Payments_OrderId_Pending")
            .HasFilter("\"Status\" = 0")
            .IsUnique();
    }
}