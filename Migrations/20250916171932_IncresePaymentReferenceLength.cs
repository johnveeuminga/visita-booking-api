using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class IncresePaymentReferenceLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PaymentReference",
                table: "BookingPayments",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "BookingPayments",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PHP",
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4360));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(5000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(5000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 19, 31, 884, DateTimeKind.Utc).AddTicks(5000));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PaymentReference",
                table: "BookingPayments",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "BookingPayments",
                type: "varchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PHP");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(6650));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7280));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7280));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7280));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7290));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7300));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 16, 17, 18, 49, 717, DateTimeKind.Utc).AddTicks(7300));
        }
    }
}
