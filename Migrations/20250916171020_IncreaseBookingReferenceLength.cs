using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseBookingReferenceLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(920));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1510));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 10, 19, 779, DateTimeKind.Utc).AddTicks(1530));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(4470));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5050));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 8, 16, 59, 372, DateTimeKind.Utc).AddTicks(5060));
        }
    }
}
