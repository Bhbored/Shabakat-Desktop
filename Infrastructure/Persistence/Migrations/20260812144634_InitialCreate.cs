using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shabakat.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmpereSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HoursPerDay = table.Column<int>(type: "INTEGER", nullable: false),
                    PricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ResidentialPricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CommercialPricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IndustrialPricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmpereSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PricePerKilowat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FixedCharge = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ResidentialPricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ResidentialPricePerKilowat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ResidentialFixedCharge = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ResidentialTVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CommercialPricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CommercialPricePerKilowat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CommercialFixedCharge = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CommercialTVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IndustrialPricePerAmp = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IndustrialPricePerKilowat = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IndustrialFixedCharge = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IndustrialTVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AmpereSchedulePricingEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AmpereProrateByDaysEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    DueDate = table.Column<int>(type: "INTEGER", nullable: false),
                    TriggerDate = table.Column<int>(type: "INTEGER", nullable: false),
                    TriggerMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpenseType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ExpenseDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.CheckConstraint("CK_Expenses_Amount", "\"Amount\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "DistributionBoxes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AreaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LocationNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionBoxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributionBoxes_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Building = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Floor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CableName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BoxId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AmpereScheduleId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CustomerType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerRelation = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SubscriptionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PriceOverride = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    FixedChargeOverride = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    TVAOverride = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CustomerStatus = table.Column<byte>(type: "INTEGER", nullable: false),
                    Plan = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PlanValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AreaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.CheckConstraint("CK_Customers_PricingOverrides_AllOrNothing", "(\r\n    \"PriceOverride\" IS NULL AND \"FixedChargeOverride\" IS NULL AND \"TVAOverride\" IS NULL\r\n)\r\nOR\r\n(\r\n    \"PriceOverride\" IS NOT NULL AND \"FixedChargeOverride\" IS NOT NULL AND \"TVAOverride\" IS NOT NULL\r\n)");
                    table.ForeignKey(
                        name: "FK_Customers_AmpereSchedules_AmpereScheduleId",
                        column: x => x.AmpereScheduleId,
                        principalTable: "AmpereSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Customers_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Customers_DistributionBoxes_BoxId",
                        column: x => x.BoxId,
                        principalTable: "DistributionBoxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FixedCharge = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TVA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BilledConsumption = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AmountDue = table.Column<decimal>(type: "decimal(18,4)", nullable: false, computedColumnSql: "\"TotalAmount\" - \"PaidAmount\"", stored: true),
                    InvoiceStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.CheckConstraint("CK_Invoices_PaidAmount", "\"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"TotalAmount\"");
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceSkips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BillingPeriodStart = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    BillingPeriodEnd = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSkips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceSkips_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReadingValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReadingDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IsInitial = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PeriodYearMonth = table.Column<int>(type: "INTEGER", nullable: false, computedColumnSql: "(CAST(strftime('%Y', \"ReadingDate\") AS INTEGER) * 100 + CAST(strftime('%m', \"ReadingDate\") AS INTEGER))", stored: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReadings_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_Payments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmpereSchedules_HoursPerDay",
                table: "AmpereSchedules",
                column: "HoursPerDay",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Username",
                table: "AppUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Name",
                table: "Areas",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "Action", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AmpereScheduleId",
                table: "Customers",
                column: "AmpereScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AreaId",
                table: "Customers",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BoxId",
                table: "Customers",
                column: "BoxId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Phone",
                table: "Customers",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionBoxes_AreaId",
                table: "DistributionBoxes",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionBoxes_Name",
                table: "DistributionBoxes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseDate",
                table: "Expenses",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UQ_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_InvoiceSkips_Customer_Period",
                table: "InvoiceSkips",
                columns: new[] { "CustomerId", "BillingPeriodStart", "BillingPeriodEnd" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_MeterReadings_CustomerId_IsInitial",
                table: "MeterReadings",
                column: "CustomerId",
                unique: true,
                filter: "\"IsInitial\" = 1");

            migrationBuilder.CreateIndex(
                name: "UQ_MeterReadings_CustomerId_PeriodYearMonth",
                table: "MeterReadings",
                columns: new[] { "CustomerId", "PeriodYearMonth" },
                unique: true,
                filter: "\"IsInitial\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerId",
                table: "Payments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPreferences");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "InvoiceSkips");

            migrationBuilder.DropTable(
                name: "MeterReadings");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "AmpereSchedules");

            migrationBuilder.DropTable(
                name: "DistributionBoxes");

            migrationBuilder.DropTable(
                name: "Areas");
        }
    }
}
