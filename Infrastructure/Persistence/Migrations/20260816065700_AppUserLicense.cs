using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shabakat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AppUserLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppUsers_Username",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "AppUsers");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LicensedUntil",
                table: "AppUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensedUntil",
                table: "AppUsers");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "AppUsers",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Username",
                table: "AppUsers",
                column: "Username",
                unique: true);
        }
    }
}
