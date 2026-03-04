using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PolicyVersionAndEndorsement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Currencies_CurrencyId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "BasePremium",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "FinalPremium",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Policies");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Policies",
                newName: "PolicyVersionId");

            migrationBuilder.AddColumn<Guid>(
                name: "PolicyVersionId",
                table: "PolicyAdjustements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrencyId",
                table: "Policies",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "CurrentVersionNumber",
                table: "Policies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PolicyEndorsements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolicyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EndorsementType = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveDate = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    OldVersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    NewVersionNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyEndorsements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyEndorsements_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicyVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PolicyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<string>(type: "TEXT", nullable: false),
                    EndDate = table.Column<string>(type: "TEXT", nullable: false),
                    BasePremium = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FinalPremium = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    IsActiveVersion = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyVersions_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyVersions_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAdjustements_PolicyVersionId",
                table: "PolicyAdjustements",
                column: "PolicyVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyEndorsements_PolicyId",
                table: "PolicyEndorsements",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyVersions_CurrencyId",
                table: "PolicyVersions",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyVersions_PolicyId",
                table: "PolicyVersions",
                column: "PolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Currencies_CurrencyId",
                table: "Policies",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyAdjustements_PolicyVersions_PolicyVersionId",
                table: "PolicyAdjustements",
                column: "PolicyVersionId",
                principalTable: "PolicyVersions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Currencies_CurrencyId",
                table: "Policies");

            migrationBuilder.DropForeignKey(
                name: "FK_PolicyAdjustements_PolicyVersions_PolicyVersionId",
                table: "PolicyAdjustements");

            migrationBuilder.DropTable(
                name: "PolicyEndorsements");

            migrationBuilder.DropTable(
                name: "PolicyVersions");

            migrationBuilder.DropIndex(
                name: "IX_PolicyAdjustements_PolicyVersionId",
                table: "PolicyAdjustements");

            migrationBuilder.DropColumn(
                name: "PolicyVersionId",
                table: "PolicyAdjustements");

            migrationBuilder.DropColumn(
                name: "CurrentVersionNumber",
                table: "Policies");

            migrationBuilder.RenameColumn(
                name: "PolicyVersionId",
                table: "Policies",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrencyId",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePremium",
                table: "Policies",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Policies",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "CancelledAt",
                table: "Policies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EndDate",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPremium",
                table: "Policies",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "StartDate",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Currencies_CurrencyId",
                table: "Policies",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
