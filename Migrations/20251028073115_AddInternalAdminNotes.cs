using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalAdminNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccommodationComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AccommodationId = table.Column<int>(type: "int", nullable: false),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationComments_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccommodationComments_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(8430));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9450));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9520));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 10, 28, 7, 31, 15, 450, DateTimeKind.Utc).AddTicks(9540));

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationComments_AccommodationId",
                table: "AccommodationComments",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationComments_AdminId",
                table: "AccommodationComments",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationComments_CreatedAt",
                table: "AccommodationComments",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationComments");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(130));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1010));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1010));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1010));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1010));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1010));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1010));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1010));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1020));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1020));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1020));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1020));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1020));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1020));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 26, 3, 55, 13, 351, DateTimeKind.Utc).AddTicks(1020));
        }
    }
}
