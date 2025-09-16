using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddAccommodationApprovalAndBusinessDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Accommodations",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "Accommodations",
                type: "int",
                nullable: true);

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

            migrationBuilder.AddColumn<string>(
                name: "BusinessNotes",
                table: "Accommodations",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPermitNumber",
                table: "Accommodations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessPermitS3Key",
                table: "Accommodations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DotAccreditationNumber",
                table: "Accommodations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DotAccreditationS3Key",
                table: "Accommodations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBtcMember",
                table: "Accommodations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtherDocumentsS3Key",
                table: "Accommodations",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Accommodations",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Accommodations",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_ApprovedById",
                table: "Accommodations",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Accommodations_Users_ApprovedById",
                table: "Accommodations",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accommodations_Users_ApprovedById",
                table: "Accommodations");

            migrationBuilder.DropIndex(
                name: "IX_Accommodations_ApprovedById",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "BtcMembershipNumber",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "BtcMembershipS3Key",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "BusinessNotes",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "BusinessPermitNumber",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "BusinessPermitS3Key",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "DotAccreditationNumber",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "DotAccreditationS3Key",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "IsBtcMember",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "OtherDocumentsS3Key",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Accommodations");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(718));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1663));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1667));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1669));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1670));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1676));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1678));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1679));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1680));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1683));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1684));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1685));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1686));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1688));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1689));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1690));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1691));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1693));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1695));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 15, 12, 26, 59, 809, DateTimeKind.Utc).AddTicks(1712));
        }
    }
}
