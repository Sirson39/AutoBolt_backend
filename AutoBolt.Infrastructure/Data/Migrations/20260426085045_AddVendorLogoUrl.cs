using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoBolt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorLogoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Vendors",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Vendors");
        }
    }
}
