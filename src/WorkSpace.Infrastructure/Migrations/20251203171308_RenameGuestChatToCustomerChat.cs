using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGuestChatToCustomerChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestChatMessages");

            migrationBuilder.DropTable(
                name: "GuestChatSessions");

            migrationBuilder.CreateTable(
                name: "CustomerChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedOwnerId = table.Column<int>(type: "int", nullable: true),
                    LastMessageAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerChatSessions_Users_AssignedOwnerId",
                        column: x => x.AssignedOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CustomerChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerChatSessionId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerChatMessages_CustomerChatSessions_CustomerChatSessionId",
                        column: x => x.CustomerChatSessionId,
                        principalTable: "CustomerChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerChatMessages_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChatMessages_CreateUtc",
                table: "CustomerChatMessages",
                column: "CreateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChatMessages_OwnerId",
                table: "CustomerChatMessages",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChatMessages_SessionId",
                table: "CustomerChatMessages",
                column: "CustomerChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChatSessions_AssignedOwnerId",
                table: "CustomerChatSessions",
                column: "AssignedOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChatSessions_IsActive",
                table: "CustomerChatSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChatSessions_SessionId",
                table: "CustomerChatSessions",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerChatMessages");

            migrationBuilder.DropTable(
                name: "CustomerChatSessions");

            migrationBuilder.CreateTable(
                name: "GuestChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignedOwnerId = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    GuestEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastMessageAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestChatSessions_Users_AssignedOwnerId",
                        column: x => x.AssignedOwnerId,
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
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
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
                        name: "FK_GuestChatMessages_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatMessages_CreateUtc",
                table: "GuestChatMessages",
                column: "CreateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatMessages_OwnerId",
                table: "GuestChatMessages",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatMessages_SessionId",
                table: "GuestChatMessages",
                column: "GuestChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestChatSessions_AssignedOwnerId",
                table: "GuestChatSessions",
                column: "AssignedOwnerId");

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
    }
}
