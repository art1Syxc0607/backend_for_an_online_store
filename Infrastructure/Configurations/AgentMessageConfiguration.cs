using Domain.Entities.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class AgentMessageConfiguration
    : IEntityTypeConfiguration<AgentMessage>
{
    public void Configure(EntityTypeBuilder<AgentMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.Text)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        // ✅ Связь с Conversation
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ Индекс для быстрой выборки
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt });
    }
}