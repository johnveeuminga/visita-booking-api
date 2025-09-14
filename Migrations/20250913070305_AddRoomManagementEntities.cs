using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomManagementEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ParentAmenityId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Amenities_Amenities_ParentAmenityId",
                        column: x => x.ParentAmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HolidayCalendar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Country = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    IsNational = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HolidayType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayCalendar", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: true),
                    CacheVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomAmenities",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    AmenityId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAmenities", x => new { x.RoomId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_RoomAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomAmenities_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomAvailabilityOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OverridePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Reason = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAvailabilityOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomAvailabilityOverrides_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    S3Key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    S3Url = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    CdnUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AltText = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomPhotos_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RoomPricingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    FixedPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MinimumPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaximumPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    MinimumNights = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomPricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomPricingRules_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayOrder", "Icon", "IsActive", "Name", "ParentAmenityId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 1, "ac-unit", true, "Air Conditioning", null },
                    { 2, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 2, "whatshot", true, "Heating", null },
                    { 3, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 3, "deck", true, "Balcony", null },
                    { 4, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 4, "location-city", true, "City View", null },
                    { 5, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 5, "waves", true, "Ocean View", null },
                    { 6, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 1, "wifi", true, "Free WiFi", null },
                    { 7, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 2, "tv", true, "Smart TV", null },
                    { 8, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 3, "usb", true, "USB Charging Ports", null },
                    { 9, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 4, "speaker", true, "Bluetooth Speaker", null },
                    { 10, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 1, "bathroom", true, "Private Bathroom", null },
                    { 11, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 2, "shower", true, "Shower", null },
                    { 12, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 3, "bathtub", true, "Bathtub", null },
                    { 13, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 4, "dry", true, "Hair Dryer", null },
                    { 14, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 5, "soap", true, "Toiletries", null },
                    { 15, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 1, "kitchen", true, "Mini Fridge", null },
                    { 16, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 2, "coffee-maker", true, "Coffee Maker", null },
                    { 17, 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 3, "microwave", true, "Microwave", null },
                    { 18, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 1, "gpp-good", true, "Safe", null },
                    { 19, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 2, "smoke-free", true, "Smoke Detector", null },
                    { 20, 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "", 3, "medical-services", true, "First Aid Kit", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Category",
                table: "Amenities",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_IsActive",
                table: "Amenities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_ParentAmenityId",
                table: "Amenities",
                column: "ParentAmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_HolidayCalendar_Date_Country",
                table: "HolidayCalendar",
                columns: new[] { "Date", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_HolidayCalendar_IsActive",
                table: "HolidayCalendar",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAmenities_AmenityId",
                table: "RoomAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAvailabilityOverrides_RoomId_Date",
                table: "RoomAvailabilityOverrides",
                columns: new[] { "RoomId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomPhotos_IsActive",
                table: "RoomPhotos",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RoomPhotos_RoomId_DisplayOrder",
                table: "RoomPhotos",
                columns: new[] { "RoomId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RoomPricingRules_RoomId_RuleType_IsActive",
                table: "RoomPricingRules",
                columns: new[] { "RoomId", "RuleType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RoomPricingRules_StartDate_EndDate",
                table: "RoomPricingRules",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HotelId",
                table: "Rooms",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_IsActive",
                table: "Rooms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Name",
                table: "Rooms",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HolidayCalendar");

            migrationBuilder.DropTable(
                name: "RoomAmenities");

            migrationBuilder.DropTable(
                name: "RoomAvailabilityOverrides");

            migrationBuilder.DropTable(
                name: "RoomPhotos");

            migrationBuilder.DropTable(
                name: "RoomPricingRules");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
