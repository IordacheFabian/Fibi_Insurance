using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedBroker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Broker_BrokerId",
                table: "Policies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Broker",
                table: "Broker");

            migrationBuilder.RenameTable(
                name: "Broker",
                newName: "Brokers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Brokers",
                table: "Brokers",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Brokers",
                columns: new[] { "Id", "BrokerCode", "BrokerStatus", "CommissionPrecentage", "Email", "Name", "PhoneNumber" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999999"), "BRK-001", 0, 10.00m, "broker@insurance.local", "Default Broker", "0700000000" });

            migrationBuilder.CreateIndex(
                name: "IX_Brokers_BrokerCode",
                table: "Brokers",
                column: "BrokerCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Brokers_BrokerId",
                table: "Policies",
                column: "BrokerId",
                principalTable: "Brokers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Brokers_BrokerId",
                table: "Policies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Brokers",
                table: "Brokers");

            migrationBuilder.DropIndex(
                name: "IX_Brokers_BrokerCode",
                table: "Brokers");

            migrationBuilder.DeleteData(
                table: "Brokers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.RenameTable(
                name: "Brokers",
                newName: "Broker");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Broker",
                table: "Broker",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Broker_BrokerId",
                table: "Policies",
                column: "BrokerId",
                principalTable: "Broker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
