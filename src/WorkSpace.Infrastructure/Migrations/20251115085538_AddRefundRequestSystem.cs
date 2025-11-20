using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundRequestSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentTransactionId",
                table: "Bookings",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    RequestingStaffId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OwnerConfirmationTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ProcessedTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StaffNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OwnerNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RefundTransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NonRefundableFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CalculatedRefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SystemCut = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Users_RequestingStaffId",
                        column: x => x.RequestingStaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_BookingId",
                table: "RefundRequests",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_RequestingStaffId",
                table: "RefundRequests",
                column: "RequestingStaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefundRequests");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                table: "Bookings");
        }
    }
}
