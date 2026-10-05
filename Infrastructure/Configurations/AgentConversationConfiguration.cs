using Domain.Entities.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations;


public class AgentConversationConfiguration
    : IEntityTypeConfiguration<AgentConversationEntity>
{
    public void Configure(EntityTypeBuilder<AgentConversationEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AgentConversationId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Title)
            .HasMaxLength(200);

        builder.Property(c => c.JsonState)
            .IsRequired()
            .HasColumnType("text");

        // ✅ Связь с User
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);  // ← удаляем диалоги с юзером

        // ✅ Индексы
        builder.HasIndex(c => c.AgentConversationId).IsUnique();
        builder.HasIndex(c => new { c.UserId, c.UpdatedAt });

        // ✅ Добавляем индекс на LastMessageAt для cleanup
        builder.HasIndex(c => new { c.UserId, c.LastMessageAt })
            .HasFilter("\"UserId\" IS NULL")  // ← только для гостей
            .HasDatabaseName("IX_AgentConversations_Guest_LastMessageAt");
    }
}
