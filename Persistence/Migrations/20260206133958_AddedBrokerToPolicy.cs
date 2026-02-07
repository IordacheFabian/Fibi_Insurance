using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedBrokerToPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Broker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BrokerCode = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    BrokerStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CommissionPrecentage = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Broker", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_BrokerId",
                table: "Policies",
                column: "BrokerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_Broker_BrokerId",
                table: "Policies",
                column: "BrokerId",
                principalTable: "Broker",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_Broker_BrokerId",
                table: "Policies");

            migrationBuilder.DropTable(
                name: "Broker");

            migrationBuilder.DropIndex(
                name: "IX_Policies_BrokerId",
                table: "Policies");
        }
    }
}
