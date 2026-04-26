using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoBolt.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOTPFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationOTP",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OTPExpiryTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationOTP",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OTPExpiryTime",
                table: "AspNetUsers");
        }
    }
}
