using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturedEventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Parks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "featured_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    image_url_2 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    image_url_3 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    image_url_4 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    image_url_5 = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    event_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_featured_events", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "park_images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ParkId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_park_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_park_images_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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

            migrationBuilder.CreateIndex(
                name: "IX_park_images_ParkId_DisplayOrder",
                table: "park_images",
                columns: new[] { "ParkId", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "featured_events");

            migrationBuilder.DropTable(
                name: "park_images");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Parks");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(7770));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8690));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8690));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8690));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8720));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 12, 20, 4, 1, 58, 956, DateTimeKind.Utc).AddTicks(8720));
        }
    }
}
