using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HostProfileDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "CustomerChatSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HostProfileDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostProfileId = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostProfileDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostProfileDocuments_HostProfiles_HostProfileId",
                        column: x => x.HostProfileId,
                        principalTable: "HostProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerChatSessions_CustomerId",
                table: "CustomerChatSessions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_HostProfileDocuments_HostProfileId",
                table: "HostProfileDocuments",
                column: "HostProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerChatSessions_Users_CustomerId",
                table: "CustomerChatSessions",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerChatSessions_Users_CustomerId",
                table: "CustomerChatSessions");

            migrationBuilder.DropTable(
                name: "HostProfileDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CustomerChatSessions_CustomerId",
                table: "CustomerChatSessions");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "CustomerChatSessions");
        }
    }
}
