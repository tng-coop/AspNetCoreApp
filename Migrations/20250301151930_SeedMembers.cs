using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AspNetCoreApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Members",
                columns: new[] { "Id", "Email", "FirstName", "JoinedDate", "LastName" },
                values: new object[,]
                {
                    { 1, "john.doe@example.com", "John", new DateTime(2025, 2, 19, 15, 19, 29, 516, DateTimeKind.Utc).AddTicks(3891), "Doe" },
                    { 2, "jane.smith@example.com", "Jane", new DateTime(2025, 2, 24, 15, 19, 29, 516, DateTimeKind.Utc).AddTicks(4209), "Smith" },
                    { 3, "alice.johnson@example.com", "Alice", new DateTime(2025, 2, 27, 15, 19, 29, 516, DateTimeKind.Utc).AddTicks(4215), "Johnson" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
