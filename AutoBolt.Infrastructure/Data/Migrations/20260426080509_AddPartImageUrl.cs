using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoBolt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Parts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Parts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Parts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Parts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Parts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Parts");

            migrationBuilder.InsertData(
                table: "Parts",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "Name", "Price", "StockQuantity", "UpdatedAt", "VendorId" },
                values: new object[,]
                {
                    { 1, 0, new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "V6 Engine Gasket", 124.50m, 8, null, null },
                    { 2, 1, new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "Brake Pads Set", 85.00m, 25, null, null },
                    { 3, 0, new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "Oil Filter", 15.99m, 50, null, null },
                    { 4, 2, new DateTime(2026, 4, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "Suspension Strut", 210.00m, 4, null, null }
                });
        }
    }
}
