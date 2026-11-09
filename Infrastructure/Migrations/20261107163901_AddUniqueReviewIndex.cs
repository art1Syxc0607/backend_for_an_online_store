using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueReviewIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Перед созданием уникального индекса убираем уже существующие дубли.
            // Оставляем самую раннюю запись (минимальный Id) для каждой пары
            // (UserId, ProductId), остальные удаляем.
            migrationBuilder.Sql(
                @"DELETE FROM ""Reviews"" AS target
                  USING ""Reviews"" AS duplicate
                  WHERE target.""UserId"" = duplicate.""UserId""
                    AND target.""ProductId"" = duplicate.""ProductId""
                    AND target.""Id"" > duplicate.""Id"";");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }
    }
}
