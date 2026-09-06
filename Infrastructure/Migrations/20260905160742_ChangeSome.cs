using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "_imageUrls",
                table: "Products",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]); // ← пустой массив по умолчанию

            migrationBuilder.AddColumn<List<string>>(
                name: "_videoUrls",
                table: "Products",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]); // ← пустой массив по умолчанию
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_imageUrls",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "_videoUrls",
                table: "Products");
        }
    }
}
