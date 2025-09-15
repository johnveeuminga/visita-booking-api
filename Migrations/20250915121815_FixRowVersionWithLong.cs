using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class FixRowVersionWithLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(5906));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6807));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6811));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6813));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6815));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6821));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6823));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6825));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6826));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6829));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6830));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6832));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6833));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6835));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6836));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6838));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6839));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6841));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6843));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 18, 15, 115, DateTimeKind.Utc).AddTicks(6844));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(623));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1412));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1427));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1430));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1431));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1437));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1438));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1440));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1441));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1443));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1444));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1446));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1447));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1448));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1451));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1452));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1454));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1455));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 15, 40, 646, DateTimeKind.Utc).AddTicks(1456));
        }
    }
}
