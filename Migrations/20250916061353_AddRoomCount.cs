using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BtcMembershipNumber",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "BtcMembershipS3Key",
                table: "Accommodations");

            migrationBuilder.AddColumn<int>(
                name: "TotalUnits",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AvailableCount",
                table: "RoomAvailabilityOverrides",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "BookingReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "BookingAvailabilityLocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4390));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 6, 13, 53, 28, DateTimeKind.Utc).AddTicks(4970));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalUnits",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "AvailableCount",
                table: "RoomAvailabilityOverrides");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "BookingReservations");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "BookingAvailabilityLocks");

            migrationBuilder.AddColumn<string>(
                name: "BtcMembershipNumber",
                table: "Accommodations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BtcMembershipS3Key",
                table: "Accommodations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1752));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1754));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1756));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1757));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1778));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1779));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1781));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1782));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1784));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1786));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1787));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1788));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1789));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1791));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1792));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1793));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1796));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1797));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 2, 26, 25, 127, DateTimeKind.Utc).AddTicks(1798));
        }
    }
}
