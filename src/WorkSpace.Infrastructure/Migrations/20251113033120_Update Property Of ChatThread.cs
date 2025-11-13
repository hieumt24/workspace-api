using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePropertyOfChatThread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "ChatThreads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "ChatThreads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasUnreadMessages",
                table: "ChatThreads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HostUserId",
                table: "ChatThreads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastMessagePreview",
                table: "ChatThreads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastMessageUtc",
                table: "ChatThreads",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_BookingId",
                table: "ChatThreads",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_CustomerId",
                table: "ChatThreads",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_HostUserId",
                table: "ChatThreads",
                column: "HostUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_Bookings_BookingId",
                table: "ChatThreads",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_Users_CustomerId",
                table: "ChatThreads",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatThreads_Users_HostUserId",
                table: "ChatThreads",
                column: "HostUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_Bookings_BookingId",
                table: "ChatThreads");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_Users_CustomerId",
                table: "ChatThreads");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatThreads_Users_HostUserId",
                table: "ChatThreads");

            migrationBuilder.DropIndex(
                name: "IX_ChatThreads_BookingId",
                table: "ChatThreads");

            migrationBuilder.DropIndex(
                name: "IX_ChatThreads_CustomerId",
                table: "ChatThreads");

            migrationBuilder.DropIndex(
                name: "IX_ChatThreads_HostUserId",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "HasUnreadMessages",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "HostUserId",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "LastMessagePreview",
                table: "ChatThreads");

            migrationBuilder.DropColumn(
                name: "LastMessageUtc",
                table: "ChatThreads");
        }
    }
}
