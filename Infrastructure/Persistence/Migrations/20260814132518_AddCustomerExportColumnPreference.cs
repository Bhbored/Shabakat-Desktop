using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shabakat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerExportColumnPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerExportColumnPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AppPreferencesId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<bool>(type: "INTEGER", nullable: false),
                    Phone = table.Column<bool>(type: "INTEGER", nullable: false),
                    Address = table.Column<bool>(type: "INTEGER", nullable: false),
                    Building = table.Column<bool>(type: "INTEGER", nullable: false),
                    Floor = table.Column<bool>(type: "INTEGER", nullable: false),
                    CableName = table.Column<bool>(type: "INTEGER", nullable: false),
                    AreaName = table.Column<bool>(type: "INTEGER", nullable: false),
                    BoxName = table.Column<bool>(type: "INTEGER", nullable: false),
                    AmpereScheduleName = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustomerType = table.Column<bool>(type: "INTEGER", nullable: false),
                    Plan = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlanValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    SubscriptionDate = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustomerStatus = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustomerRelation = table.Column<bool>(type: "INTEGER", nullable: false),
                    InitialMeterReading = table.Column<bool>(type: "INTEGER", nullable: false),
                    LatestMeterReading = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalBilled = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalToPay = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerExportColumnPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerExportColumnPreferences_AppPreferences_AppPreferencesId",
                        column: x => x.AppPreferencesId,
                        principalTable: "AppPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerExportColumnPreferences_AppPreferencesId",
                table: "CustomerExportColumnPreferences",
                column: "AppPreferencesId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerExportColumnPreferences");
        }
    }
}
