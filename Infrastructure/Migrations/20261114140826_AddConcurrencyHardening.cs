using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            // xmin is a PostgreSQL system column: EF only needs it in the model
            // as an optimistic concurrency token, it must not be created here.

            // Orders created before this fix may already have several pending
            // payment attempts. Keep the newest one and fail the rest so the
            // unique index below can be created.
            migrationBuilder.Sql(
                """
                UPDATE "Payments" AS p
                SET "Status" = 2
                WHERE p."Status" = 0
                  AND EXISTS (
                      SELECT 1
                      FROM "Payments" AS newer
                      WHERE newer."OrderId" = p."OrderId"
                        AND newer."Status" = 0
                        AND newer."Id" > p."Id"
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId_Pending",
                table: "Payments",
                column: "OrderId",
                unique: true,
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId_Pending",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");
        }
    }
}
