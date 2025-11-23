using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "Establishments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Logo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CoverImage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, defaultValue: "Baguio"),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: true),
                    ContactNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Website = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    FacebookPage = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    BusinessPermitS3Key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    RejectionReason = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Establishments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Establishments_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Establishments_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EstablishmentComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EstablishmentId = table.Column<int>(type: "int", nullable: false),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstablishmentComments_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstablishmentComments_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EstablishmentHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EstablishmentId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstablishmentHours_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(690));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1580));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 11, 5, 5, 35, 33, 229, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentComments_AdminId",
                table: "EstablishmentComments",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentComments_CreatedAt",
                table: "EstablishmentComments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentComments_EstablishmentId",
                table: "EstablishmentComments",
                column: "EstablishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentHours_DayOfWeek",
                table: "EstablishmentHours",
                column: "DayOfWeek");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentHours_EstablishmentId",
                table: "EstablishmentHours",
                column: "EstablishmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_ApprovedById",
                table: "Establishments",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Category",
                table: "Establishments",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_City",
                table: "Establishments",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_IsActive",
                table: "Establishments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Name",
                table: "Establishments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_OwnerId",
                table: "Establishments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Status",
                table: "Establishments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstablishmentComments");

            migrationBuilder.DropTable(
                name: "EstablishmentHours");

            migrationBuilder.DropTable(
                name: "Establishments");

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
    }
}
