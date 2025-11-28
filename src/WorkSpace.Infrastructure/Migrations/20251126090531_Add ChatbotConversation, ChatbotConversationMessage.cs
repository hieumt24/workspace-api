using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatbotConversationChatbotConversationMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatbotConversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageCount = table.Column<int>(type: "int", nullable: false),
                    ChatbotConversationId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatbotConversations_ChatbotConversations_ChatbotConversationId",
                        column: x => x.ChatbotConversationId,
                        principalTable: "ChatbotConversations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatbotConversations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatbotConversationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    ExtractedIntentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendationCount = table.Column<int>(type: "int", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatbotConversationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatbotConversationMessages_ChatbotConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ChatbotConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotConversationMessages_ConversationId",
                table: "ChatbotConversationMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotConversations_ChatbotConversationId",
                table: "ChatbotConversations",
                column: "ChatbotConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatbotConversations_UserId",
                table: "ChatbotConversations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatbotConversationMessages");

            migrationBuilder.DropTable(
                name: "ChatbotConversations");
        }
    }
}
