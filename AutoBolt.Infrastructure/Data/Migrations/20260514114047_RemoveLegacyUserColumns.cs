using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoBolt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyUserColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" DROP COLUMN IF EXISTS \"Role\";");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" DROP COLUMN IF EXISTS \"CreatedAt\";");
            migrationBuilder.Sql("ALTER TABLE \"AspNetUsers\" DROP COLUMN IF EXISTS \"UpdatedAt\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
