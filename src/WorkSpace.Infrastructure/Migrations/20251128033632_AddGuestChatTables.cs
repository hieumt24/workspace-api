using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestChatTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuestChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedStaffId = table.Column<int>(type: "int", nullable: true),
                    LastMessageAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestChatSessions_Users_AssignedStaffId",
                        column: x => x.AssignedStaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GuestChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestChatSessionId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsStaff = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestChatMessages_GuestChatSessions_GuestChatSessionId",
                        column: x => x.GuestChatSessionId,
                        principalTable: "GuestChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestChatMessages_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatMessages_CreateUtc",
                table: "GuestChatMessages",
                column: "CreateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatMessages_SessionId",
                table: "GuestChatMessages",
                column: "GuestChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatMessages_StaffId",
                table: "GuestChatMessages",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatSessions_AssignedStaffId",
                table: "GuestChatSessions",
                column: "AssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatSessions_IsActive",
                table: "GuestChatSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatSessions_SessionId",
                table: "GuestChatSessions",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestChatMessages");

            migrationBuilder.DropTable(
                name: "GuestChatSessions");
        }
    }
}
