using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddAccommodationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAvailabilityLocks_User_UserId",
                table: "BookingAvailabilityLocks");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingReservations_User_UserId",
                table: "BookingReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_User_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Hotels_HotelId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "Rooms",
                newName: "AccommodationId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_HotelId",
                table: "Rooms",
                newName: "IX_Rooms_AccommodationId");

            migrationBuilder.CreateTable(
                name: "Accommodations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Logo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accommodations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accommodations_Users_OwnerId",
                        column: x => x.OwnerId,
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
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(6487));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7255));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7259));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7260));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7262));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7267));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7269));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7270));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7271));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7273));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7275));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7276));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7277));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7279));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7280));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7281));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7283));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7284));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7286));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 14, 8, 38, 46, 598, DateTimeKind.Utc).AddTicks(7287));

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_IsActive",
                table: "Accommodations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_Name",
                table: "Accommodations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_OwnerId",
                table: "Accommodations",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAvailabilityLocks_Users_UserId",
                table: "BookingAvailabilityLocks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingReservations_Users_UserId",
                table: "BookingReservations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Accommodations_AccommodationId",
                table: "Rooms",
                column: "AccommodationId",
                principalTable: "Accommodations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAvailabilityLocks_Users_UserId",
                table: "BookingAvailabilityLocks");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingReservations_Users_UserId",
                table: "BookingReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Accommodations_AccommodationId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "Accommodations");

            migrationBuilder.RenameColumn(
                name: "AccommodationId",
                table: "Rooms",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Rooms_AccommodationId",
                table: "Rooms",
                newName: "IX_Rooms_HotelId");

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    BusinessRegistrationNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CheckInTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    CheckOutTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    HasGym = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasParking = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasPool = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasRestaurant = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasSpa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasWifi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Website = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hotels_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "longtext", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FirstName = table.Column<string>(type: "longtext", nullable: false),
                    GoogleId = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastName = table.Column<string>(type: "longtext", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "longtext", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "longtext", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 708, DateTimeKind.Utc).AddTicks(8710));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(221));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(226));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(228));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(230));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(239));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(241));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(243));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(244));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(246));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(248));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(249));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(268));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(270));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(272));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(279));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(280));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(283));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(284));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 13, 45, 18, 709, DateTimeKind.Utc).AddTicks(286));

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_City",
                table: "Hotels",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_IsActive",
                table: "Hotels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_IsVerified",
                table: "Hotels",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Name",
                table: "Hotels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_OwnerId",
                table: "Hotels",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Rating",
                table: "Hotels",
                column: "Rating");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAvailabilityLocks_User_UserId",
                table: "BookingAvailabilityLocks",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingReservations_User_UserId",
                table: "BookingReservations",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_User_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Hotels_HotelId",
                table: "Rooms",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
