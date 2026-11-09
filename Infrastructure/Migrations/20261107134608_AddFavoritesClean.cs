using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddFavoritesClean : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Fix state left by the previous partially-applied migration:
        // - Favorites table was created but migration was not recorded
        // - IX_Reviews_UserId index was dropped
        // - IX_Reviews_UserId_ProductId unique index failed to create
        migrationBuilder.Sql("DROP TABLE IF EXISTS \"Favorites\"");
        migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Reviews_UserId\" ON \"Reviews\" (\"UserId\")");

        migrationBuilder.CreateTable(
            name: "Favorites",
            columns: table => new
            {
                UserId = table.Column<int>(type: "integer", nullable: false),
                ProductId = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Favorites", x => new { x.UserId, x.ProductId });
                table.ForeignKey(
                    name: "FK_Favorites_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Favorites_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Favorites_ProductId",
            table: "Favorites",
            column: "ProductId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Favorites");

        migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Reviews_UserId\"");
        migrationBuilder.Sql("CREATE INDEX \"IX_Reviews_UserId\" ON \"Reviews\" (\"UserId\")");
    }
}
