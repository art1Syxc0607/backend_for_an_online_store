using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Case- and whitespace-insensitive copy of Name used only for uniqueness.
        // Kept as a shadow property so the domain entity stays clean; computed
        // columns are a relational feature, so this lives in Infrastructure.
        builder.Property<string>("NormalizedName")
            .HasComputedColumnSql(@"lower(btrim(""Name""))", stored: true);

        // Prevents duplicate categories even under concurrent inserts:
        // the second INSERT fails with 23505 which the middleware maps to 409.
        builder.HasIndex("NormalizedName")
            .IsUnique()
            .HasDatabaseName("IX_Categories_NormalizedName");
    }
}
