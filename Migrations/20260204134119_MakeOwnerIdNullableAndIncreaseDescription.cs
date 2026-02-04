using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class MakeOwnerIdNullableAndIncreaseDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Accommodations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Accommodations",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9070));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9690));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9690));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2026, 2, 4, 13, 41, 18, 883, DateTimeKind.Utc).AddTicks(9720));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Accommodations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Accommodations",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

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
    }
}
