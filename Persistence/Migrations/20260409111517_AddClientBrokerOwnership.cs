using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientBrokerOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BrokerId",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE Clients
                SET BrokerId = (
                    SELECT p.BrokerId
                    FROM Policies p
                    WHERE p.ClientId = Clients.Id
                    GROUP BY p.BrokerId
                    HAVING COUNT(DISTINCT p.BrokerId) = 1
                    LIMIT 1
                )
                WHERE Id IN (
                    SELECT p.ClientId
                    FROM Policies p
                    GROUP BY p.ClientId
                    HAVING COUNT(DISTINCT p.BrokerId) = 1
                );");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_BrokerId",
                table: "Clients",
                column: "BrokerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Brokers_BrokerId",
                table: "Clients",
                column: "BrokerId",
                principalTable: "Brokers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Brokers_BrokerId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_BrokerId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BrokerId",
                table: "Clients");
        }
    }
}
