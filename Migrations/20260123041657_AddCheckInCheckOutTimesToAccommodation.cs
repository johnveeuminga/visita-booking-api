using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInCheckOutTimesToAccommodation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckInTime",
                table: "Accommodations",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckOutTime",
                table: "Accommodations",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3030));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2026, 1, 23, 4, 16, 57, 132, DateTimeKind.Utc).AddTicks(3990));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInTime",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "CheckOutTime",
                table: "Accommodations");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(1960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2840));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2840));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2840));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2840));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2850));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2860));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2860));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2860));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2860));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2860));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2870));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2026, 1, 13, 3, 54, 30, 776, DateTimeKind.Utc).AddTicks(2870));
        }
    }
}
