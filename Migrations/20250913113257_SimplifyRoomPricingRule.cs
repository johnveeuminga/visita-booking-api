using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyRoomPricingRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumPrice",
                table: "RoomPricingRules");

            migrationBuilder.DropColumn(
                name: "MinimumPrice",
                table: "RoomPricingRules");

            migrationBuilder.DropColumn(
                name: "PriceMultiplier",
                table: "RoomPricingRules");

            migrationBuilder.AlterColumn<decimal>(
                name: "FixedPrice",
                table: "RoomPricingRules",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RoomPriceCaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    MinPrice30Days = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxPrice30Days = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AvgPrice30Days = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MinPrice90Days = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxPrice90Days = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AvgPrice90Days = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    WeekendMultiplier = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    HolidayMultiplier = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    PeakSeasonMultiplier = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    PriceBand = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastPricingRuleChange = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataValidUntil = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SearchHitCount = table.Column<int>(type: "int", nullable: false),
                    LastSearched = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomPriceCaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomPriceCaches_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(7172));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8033));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8037));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8039));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8040));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8053));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8054));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8056));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8057));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8059));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8061));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8062));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8063));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8064));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8066));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8067));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8068));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8071));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8073));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 11, 32, 57, 310, DateTimeKind.Utc).AddTicks(8074));

            migrationBuilder.CreateIndex(
                name: "IX_RoomPriceCaches_DataValidUntil",
                table: "RoomPriceCaches",
                column: "DataValidUntil");

            migrationBuilder.CreateIndex(
                name: "IX_RoomPriceCaches_LastUpdated",
                table: "RoomPriceCaches",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_RoomPriceCaches_MinPrice30Days_MaxPrice30Days",
                table: "RoomPriceCaches",
                columns: new[] { "MinPrice30Days", "MaxPrice30Days" });

            migrationBuilder.CreateIndex(
                name: "IX_RoomPriceCaches_MinPrice90Days_MaxPrice90Days",
                table: "RoomPriceCaches",
                columns: new[] { "MinPrice90Days", "MaxPrice90Days" });

            migrationBuilder.CreateIndex(
                name: "IX_RoomPriceCaches_PriceBand",
                table: "RoomPriceCaches",
                column: "PriceBand");

            migrationBuilder.CreateIndex(
                name: "IX_RoomPriceCaches_RoomId",
                table: "RoomPriceCaches",
                column: "RoomId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomPriceCaches");

            migrationBuilder.AlterColumn<decimal>(
                name: "FixedPrice",
                table: "RoomPricingRules",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumPrice",
                table: "RoomPricingRules",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumPrice",
                table: "RoomPricingRules",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceMultiplier",
                table: "RoomPricingRules",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: true);

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
        }
    }
}
