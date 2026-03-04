using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IncludePolicyVersionInPolicyAdjustement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PolicyAdjustements_Policies_PolicyId",
                table: "PolicyAdjustements");

            migrationBuilder.DropForeignKey(
                name: "FK_PolicyAdjustements_PolicyVersions_PolicyVersionId",
                table: "PolicyAdjustements");

            migrationBuilder.DropIndex(
                name: "IX_PolicyAdjustements_PolicyId_AdjustementType",
                table: "PolicyAdjustements");

            migrationBuilder.DropIndex(
                name: "IX_PolicyAdjustements_PolicyVersionId",
                table: "PolicyAdjustements");

            migrationBuilder.DropColumn(
                name: "PolicyId",
                table: "PolicyAdjustements");

            migrationBuilder.AlterColumn<Guid>(
                name: "PolicyVersionId",
                table: "PolicyAdjustements",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAdjustements_PolicyVersionId_AdjustementType",
                table: "PolicyAdjustements",
                columns: new[] { "PolicyVersionId", "AdjustementType" });

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyAdjustements_PolicyVersions_PolicyVersionId",
                table: "PolicyAdjustements",
                column: "PolicyVersionId",
                principalTable: "PolicyVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PolicyAdjustements_PolicyVersions_PolicyVersionId",
                table: "PolicyAdjustements");

            migrationBuilder.DropIndex(
                name: "IX_PolicyAdjustements_PolicyVersionId_AdjustementType",
                table: "PolicyAdjustements");

            migrationBuilder.AlterColumn<Guid>(
                name: "PolicyVersionId",
                table: "PolicyAdjustements",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<Guid>(
                name: "PolicyId",
                table: "PolicyAdjustements",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAdjustements_PolicyId_AdjustementType",
                table: "PolicyAdjustements",
                columns: new[] { "PolicyId", "AdjustementType" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAdjustements_PolicyVersionId",
                table: "PolicyAdjustements",
                column: "PolicyVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyAdjustements_Policies_PolicyId",
                table: "PolicyAdjustements",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyAdjustements_PolicyVersions_PolicyVersionId",
                table: "PolicyAdjustements",
                column: "PolicyVersionId",
                principalTable: "PolicyVersions",
                principalColumn: "Id");
        }
    }
}
