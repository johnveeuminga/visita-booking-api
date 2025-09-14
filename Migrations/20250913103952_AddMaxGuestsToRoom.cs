using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxGuestsToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxGuests",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "HolidayCalendar",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Amenities",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(6354));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7254));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7258));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7259));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7260));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7266));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7268));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7269));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7270));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7272));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7274));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7275));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7276));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7278));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7279));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7280));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7281));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7284));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7285));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 10, 39, 51, 611, DateTimeKind.Utc).AddTicks(7286));

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_MaxGuests",
                table: "Rooms",
                column: "MaxGuests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rooms_MaxGuests",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "MaxGuests",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "HolidayCalendar");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Amenities");
        }
    }
}
