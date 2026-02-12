using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreatedUpdatedAtForBroker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CommissionPrecentage",
                table: "Brokers",
                newName: "CommissionPercentage");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Brokers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Brokers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Brokers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Brokers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Brokers");

            migrationBuilder.RenameColumn(
                name: "CommissionPercentage",
                table: "Brokers",
                newName: "CommissionPrecentage");
        }
    }
}
