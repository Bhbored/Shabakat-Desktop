using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shabakat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTriggerDateAndMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriggerDate",
                table: "AppPreferences");

            migrationBuilder.DropColumn(
                name: "TriggerMessage",
                table: "AppPreferences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TriggerDate",
                table: "AppPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TriggerMessage",
                table: "AppPreferences",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }
    }
}
