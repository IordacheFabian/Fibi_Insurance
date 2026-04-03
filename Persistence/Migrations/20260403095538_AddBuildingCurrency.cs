using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingCurrency : Migration
    {
        private static readonly Guid DefaultCurrencyId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "Buildings",
                type: "TEXT",
                nullable: false,
                defaultValue: DefaultCurrencyId);

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CurrencyId",
                table: "Buildings",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Currencies_CurrencyId",
                table: "Buildings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Currencies_CurrencyId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_CurrencyId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Buildings");
        }
    }
}
