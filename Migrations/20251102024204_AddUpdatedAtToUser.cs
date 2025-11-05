using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(7790));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8680));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8680));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8680));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8680));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 982, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 964, DateTimeKind.Utc).AddTicks(3870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 964, DateTimeKind.Utc).AddTicks(5010));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2025, 11, 2, 2, 42, 3, 964, DateTimeKind.Utc).AddTicks(5010));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7890));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7890));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7900));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7900));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7900));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7900));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7900));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7910));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7920));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7920));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7920));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7920));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7930));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 10, 29, 6, 27, 48, 5, DateTimeKind.Utc).AddTicks(7930));
        }
    }
}
