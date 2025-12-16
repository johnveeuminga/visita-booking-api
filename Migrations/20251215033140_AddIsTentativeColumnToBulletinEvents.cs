using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsTentativeColumnToBulletinEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTentative",
                table: "BulletinEvents",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(4110));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5190));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5190));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5200));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5200));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5210));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5210));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5210));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5210));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5220));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5220));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5220));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5220));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5220));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5220));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5230));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5230));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5230));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5230));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 12, 15, 3, 31, 39, 983, DateTimeKind.Utc).AddTicks(5230));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTentative",
                table: "BulletinEvents");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(1830));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2730));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2740));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2760));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2760));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2760));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2760));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2760));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 12, 10, 3, 55, 43, 481, DateTimeKind.Utc).AddTicks(2760));
        }
    }
}
