using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspNetCoreApp.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "JoinedDate",
                value: new DateTime(2024, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2,
                column: "JoinedDate",
                value: new DateTime(2024, 2, 25, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3,
                column: "JoinedDate",
                value: new DateTime(2024, 2, 28, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 1,
                column: "JoinedDate",
                value: new DateTime(2025, 2, 19, 15, 19, 29, 516, DateTimeKind.Utc).AddTicks(3891));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 2,
                column: "JoinedDate",
                value: new DateTime(2025, 2, 24, 15, 19, 29, 516, DateTimeKind.Utc).AddTicks(4209));

            migrationBuilder.UpdateData(
                table: "Members",
                keyColumn: "Id",
                keyValue: 3,
                column: "JoinedDate",
                value: new DateTime(2025, 2, 27, 15, 19, 29, 516, DateTimeKind.Utc).AddTicks(4215));
        }
    }
}
