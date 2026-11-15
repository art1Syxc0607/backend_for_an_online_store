using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUniqueSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedSku",
                table: "Products",
                type: "text",
                nullable: true,
                computedColumnSql: "lower(btrim(\"Sku\"))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_NormalizedSku",
                table: "Products",
                column: "NormalizedSku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_NormalizedSku",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NormalizedSku",
                table: "Products");
        }
    }
}
