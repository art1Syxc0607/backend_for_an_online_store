using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAgentConversationRetention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AgentConversations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "GuestId",
                table: "AgentConversations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentConversations_Guest_LastMessageAt",
                table: "AgentConversations",
                columns: new[] { "UserId", "LastMessageAt" },
                filter: "\"UserId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AgentConversations_Guest_LastMessageAt",
                table: "AgentConversations");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "AgentConversations");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AgentConversations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
