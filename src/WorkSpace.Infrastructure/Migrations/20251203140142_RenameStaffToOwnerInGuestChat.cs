using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameStaffToOwnerInGuestChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestChatMessages_Users_StaffId",
                table: "GuestChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GuestChatSessions_Users_AssignedStaffId",
                table: "GuestChatSessions");

            migrationBuilder.RenameColumn(
                name: "AssignedStaffId",
                table: "GuestChatSessions",
                newName: "AssignedOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_GuestChatSessions_AssignedStaffId",
                table: "GuestChatSessions",
                newName: "IX_GuestChatSessions_AssignedOwnerId");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "GuestChatMessages",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "IsStaff",
                table: "GuestChatMessages",
                newName: "IsOwner");

            migrationBuilder.RenameIndex(
                name: "IX_GuestChatMessages_StaffId",
                table: "GuestChatMessages",
                newName: "IX_GuestChatMessages_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestChatMessages_Users_OwnerId",
                table: "GuestChatMessages",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestChatSessions_Users_AssignedOwnerId",
                table: "GuestChatSessions",
                column: "AssignedOwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestChatMessages_Users_OwnerId",
                table: "GuestChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GuestChatSessions_Users_AssignedOwnerId",
                table: "GuestChatSessions");

            migrationBuilder.RenameColumn(
                name: "AssignedOwnerId",
                table: "GuestChatSessions",
                newName: "AssignedStaffId");

            migrationBuilder.RenameIndex(
                name: "IX_GuestChatSessions_AssignedOwnerId",
                table: "GuestChatSessions",
                newName: "IX_GuestChatSessions_AssignedStaffId");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "GuestChatMessages",
                newName: "StaffId");

            migrationBuilder.RenameColumn(
                name: "IsOwner",
                table: "GuestChatMessages",
                newName: "IsStaff");

            migrationBuilder.RenameIndex(
                name: "IX_GuestChatMessages_OwnerId",
                table: "GuestChatMessages",
                newName: "IX_GuestChatMessages_StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestChatMessages_Users_StaffId",
                table: "GuestChatMessages",
                column: "StaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestChatSessions_Users_AssignedStaffId",
                table: "GuestChatSessions",
                column: "AssignedStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
