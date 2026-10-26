using Domain.Entities;
using Domain.Entities.Agent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Collections.Generic;
using System.Text.Json;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Payment> Payments { get; set; } 
    public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

    // agent
    public DbSet<AgentConversationEntity> AgentConversations { get; set; }
    public DbSet<AgentMessage> AgentMessages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // PostgreSQL system column used as an optimistic concurrency token.
        // Kept provider-specific so InMemory-based unit tests keep working.
        if (Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            builder.Entity<Product>()
                .Property<uint>("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            builder.Entity<Order>()
                .Property<uint>("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            builder.Entity<Payment>()
                .Property<uint>("xmin")
                .HasColumnType("xid")
                .IsRowVersion();
        }

        base.OnModelCreating(builder);
    }
}

