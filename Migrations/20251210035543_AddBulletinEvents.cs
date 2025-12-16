using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBulletinEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BulletinEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EventType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    LinkUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulletinEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulletinEvents_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_BulletinEvents_CreatedAt",
                table: "BulletinEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinEvents_CreatedBy",
                table: "BulletinEvents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinEvents_EndDate",
                table: "BulletinEvents",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinEvents_EventType",
                table: "BulletinEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinEvents_StartDate",
                table: "BulletinEvents",
                column: "StartDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BulletinEvents");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(7530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8420));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8420));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8420));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8430));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8430));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8430));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8440));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8440));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8440));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8440));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8440));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8460));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 12, 9, 13, 34, 9, 639, DateTimeKind.Utc).AddTicks(8460));
        }
    }
}
