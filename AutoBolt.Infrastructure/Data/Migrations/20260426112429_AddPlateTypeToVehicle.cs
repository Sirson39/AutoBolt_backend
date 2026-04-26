using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoBolt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlateTypeToVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlateType",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlateType",
                table: "Vehicles");
        }
    }
}
