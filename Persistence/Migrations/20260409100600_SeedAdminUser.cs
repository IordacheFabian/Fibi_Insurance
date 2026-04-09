using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "BrokerId", "CreatedAt", "Email", "PasswordHash", "Role", "UpdatedAt", "isActive" },
                values: new object[] { new Guid("8f5f4a82-6e4d-4de3-9f7d-6fb52e1d3e10"), null, new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Utc), "admin@insurance.local", "$2a$11$abcdefghijklmnopqrstuuoJawvMJ6sk.VuDVCPweEhcf4.1GfIAi", "Admin", null, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("8f5f4a82-6e4d-4de3-9f7d-6fb52e1d3e10"));
        }
    }
}
