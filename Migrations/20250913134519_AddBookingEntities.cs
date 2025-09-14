using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace visita_booking_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "longtext", nullable: false),
                    FirstName = table.Column<string>(type: "longtext", nullable: false),
                    LastName = table.Column<string>(type: "longtext", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    GoogleId = table.Column<string>(type: "longtext", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EmailVerificationToken = table.Column<string>(type: "longtext", nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "longtext", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BookingReference = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "date", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    NumberOfNights = table.Column<int>(type: "int", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    ServiceFee = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    GuestName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    GuestEmail = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    GuestPhone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    SpecialRequests = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CancellationReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ActualCheckInAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualCheckOutAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PaymentReference = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    XenditInvoiceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    XenditPaymentId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    XenditExternalId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    XenditPaymentUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FailureReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ProviderTransactionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ProviderPaymentMethod = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CardLastFour = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true),
                    BankCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ProviderFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    PlatformFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    NetAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    RefundedFromPaymentId = table.Column<int>(type: "int", nullable: true),
                    RefundReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    XenditWebhookData = table.Column<string>(type: "json", nullable: true),
                    ProviderMetadata = table.Column<string>(type: "json", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingPayments_BookingPayments_RefundedFromPaymentId",
                        column: x => x.RefundedFromPaymentId,
                        principalTable: "BookingPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingPayments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ReservationReference = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "date", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    XenditInvoiceId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PaymentUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    PaymentUrlExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn),
                    ExtensionCount = table.Column<int>(type: "int", nullable: false),
                    LastExtendedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancellationReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingReservations_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingReservations_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingReservations_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingAvailabilityLocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LockReference = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "date", nullable: false),
                    LockType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SessionId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReleaseReason = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAvailabilityLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingAvailabilityLocks_BookingReservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "BookingReservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookingAvailabilityLocks_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BookingAvailabilityLocks_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingAvailabilityLocks_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "IX_BookingAvailabilityLocks_BookingId",
                table: "BookingAvailabilityLocks",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_CreatedAt",
                table: "BookingAvailabilityLocks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_ExpiresAt",
                table: "BookingAvailabilityLocks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_IsActive",
                table: "BookingAvailabilityLocks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_LockReference",
                table: "BookingAvailabilityLocks",
                column: "LockReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_ReservationId",
                table: "BookingAvailabilityLocks",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_RoomId",
                table: "BookingAvailabilityLocks",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_RoomId_CheckInDate_CheckOutDate",
                table: "BookingAvailabilityLocks",
                columns: new[] { "RoomId", "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingAvailabilityLocks_UserId",
                table: "BookingAvailabilityLocks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_BookingId",
                table: "BookingPayments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_CreatedAt",
                table: "BookingPayments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_PaymentReference",
                table: "BookingPayments",
                column: "PaymentReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_PaymentType",
                table: "BookingPayments",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_RefundedFromPaymentId",
                table: "BookingPayments",
                column: "RefundedFromPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_Status",
                table: "BookingPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_XenditInvoiceId",
                table: "BookingPayments",
                column: "XenditInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingPayments_XenditPaymentId",
                table: "BookingPayments",
                column: "XenditPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_BookingId",
                table: "BookingReservations",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_ExpiresAt",
                table: "BookingReservations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_ReservationReference",
                table: "BookingReservations",
                column: "ReservationReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_ReservedAt",
                table: "BookingReservations",
                column: "ReservedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_RoomId_CheckInDate_CheckOutDate",
                table: "BookingReservations",
                columns: new[] { "RoomId", "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_Status",
                table: "BookingReservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_UserId",
                table: "BookingReservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingReservations_XenditInvoiceId",
                table: "BookingReservations",
                column: "XenditInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingReference",
                table: "Bookings",
                column: "BookingReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CheckInDate_CheckOutDate",
                table: "Bookings",
                columns: new[] { "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CreatedAt",
                table: "Bookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GuestEmail",
                table: "Bookings",
                column: "GuestEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymentStatus",
                table: "Bookings",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId",
                table: "Bookings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingAvailabilityLocks");

            migrationBuilder.DropTable(
                name: "BookingPayments");

            migrationBuilder.DropTable(
                name: "BookingReservations");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(8630));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9452));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9455));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9457));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9459));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 6,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9465));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 7,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9467));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 8,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9468));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 9,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9470));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 10,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9526));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 11,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9528));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 12,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9530));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 13,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9532));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 14,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9533));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 15,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9538));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 16,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9540));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 17,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9541));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 18,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9543));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 19,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9545));

            migrationBuilder.UpdateData(
                table: "Amenities",
                keyColumn: "Id",
                keyValue: 20,
                column: "LastModified",
                value: new DateTime(2025, 9, 13, 12, 43, 52, 433, DateTimeKind.Utc).AddTicks(9546));
        }
    }
}
