using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixChatThreadBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_Bookings_BookingId1",
                table: "ChatThreads");

            migrationBuilder.DropIndex(
                name: "IX_ChatThreads_BookingId1",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "BookingId1",
                table: "ChatThreads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingId1",
                table: "ChatThreads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_BookingId1",
                table: "ChatThreads",
                column: "BookingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_Bookings_BookingId1",
                table: "ChatThreads",
                column: "BookingId1",
                principalTable: "Bookings",
                principalColumn: "Id");
        }
    }
}
