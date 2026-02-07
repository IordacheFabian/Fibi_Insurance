using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MetadataAndPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Cities_CityId",
                table: "Addresses");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ExchangeRateToBase = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FeeType = table.Column<int>(type: "INTEGER", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: false),
                    EffectiveFrom = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskFactorConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RiskLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceID = table.Column<Guid>(type: "TEXT", nullable: true),
                    BuildingType = table.Column<int>(type: "INTEGER", nullable: true),
                    AdjustementPercentage = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskFactorConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolicyNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BuildingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolicyStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<string>(type: "TEXT", nullable: false),
                    EndDate = table.Column<string>(type: "TEXT", nullable: false),
                    BasePremium = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FinalPremium = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CancelledAt = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CancellationReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Policies_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Policies_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Policies_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyAdjustements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolicyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AdjustementType = table.Column<int>(type: "INTEGER", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyAdjustements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyAdjustements_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Counties",
                columns: new[] { "Id", "CountryId", "Name" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222223"), new Guid("11111111-1111-1111-1111-111111111111"), "Cluj" },
                    { new Guid("22222222-2222-2222-2222-222222222224"), new Guid("11111111-1111-1111-1111-111111111111"), "Timiș" },
                    { new Guid("22222222-2222-2222-2222-222222222225"), new Guid("11111111-1111-1111-1111-111111111111"), "Iași" },
                    { new Guid("22222222-2222-2222-2222-222222222226"), new Guid("11111111-1111-1111-1111-111111111111"), "Brașov" }
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "ExchangeRateToBase", "Name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "RON", 1m, "Romanian Leu" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "EUR", 4.95m, "Euro" }
                });

            migrationBuilder.InsertData(
                table: "FeeConfigurations",
                columns: new[] { "Id", "EffectiveFrom", "EffectiveTo", "FeeType", "IsActive", "Name", "Percentage" },
                values: new object[] { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "2026-01-01", null, 0, true, "Default broker commission", 0.10m });

            migrationBuilder.InsertData(
                table: "RiskFactorConfigurations",
                columns: new[] { "Id", "AdjustementPercentage", "BuildingType", "IsActive", "ReferenceID", "RiskLevel" },
                values: new object[] { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), 0.05m, null, true, new Guid("33333333-3333-3333-3333-333333333333"), 2 });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountyId", "Name" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333334"), new Guid("22222222-2222-2222-2222-222222222223"), "Cluj-Napoca" },
                    { new Guid("33333333-3333-3333-3333-333333333335"), new Guid("22222222-2222-2222-2222-222222222223"), "Florești" },
                    { new Guid("33333333-3333-3333-3333-333333333336"), new Guid("22222222-2222-2222-2222-222222222224"), "Timișoara" },
                    { new Guid("33333333-3333-3333-3333-333333333337"), new Guid("22222222-2222-2222-2222-222222222224"), "Lugoj" },
                    { new Guid("33333333-3333-3333-3333-333333333338"), new Guid("22222222-2222-2222-2222-222222222225"), "Iași" },
                    { new Guid("33333333-3333-3333-3333-333333333339"), new Guid("22222222-2222-2222-2222-222222222226"), "Brașov" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IdentificationNumber",
                table: "Clients",
                column: "IdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeeConfigurations_FeeType_EffectiveFrom",
                table: "FeeConfigurations",
                columns: new[] { "FeeType", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_BuildingId",
                table: "Policies",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_ClientId",
                table: "Policies",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CurrencyId",
                table: "Policies",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyNumber",
                table: "Policies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAdjustements_PolicyId_AdjustementType",
                table: "PolicyAdjustements",
                columns: new[] { "PolicyId", "AdjustementType" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorConfigurations_RiskLevel_BuildingType",
                table: "RiskFactorConfigurations",
                columns: new[] { "RiskLevel", "BuildingType" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskFactorConfigurations_RiskLevel_ReferenceID",
                table: "RiskFactorConfigurations",
                columns: new[] { "RiskLevel", "ReferenceID" });

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Cities_CityId",
                table: "Addresses",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Cities_CityId",
                table: "Addresses");

            migrationBuilder.DropTable(
                name: "FeeConfigurations");

            migrationBuilder.DropTable(
                name: "PolicyAdjustements");

            migrationBuilder.DropTable(
                name: "RiskFactorConfigurations");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Name",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Clients_IdentificationNumber",
                table: "Clients");

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333334"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333335"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333336"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333337"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333338"));

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333339"));

            migrationBuilder.DeleteData(
                table: "Counties",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"));

            migrationBuilder.DeleteData(
                table: "Counties",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222224"));

            migrationBuilder.DeleteData(
                table: "Counties",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222225"));

            migrationBuilder.DeleteData(
                table: "Counties",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222226"));

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Cities_CityId",
                table: "Addresses",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
