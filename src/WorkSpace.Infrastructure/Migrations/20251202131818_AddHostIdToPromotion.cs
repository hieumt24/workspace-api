using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHostIdToPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HostId",
                table: "Promotions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_HostId",
                table: "Promotions",
                column: "HostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_HostProfiles_HostId",
                table: "Promotions",
                column: "HostId",
                principalTable: "HostProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_HostProfiles_HostId",
                table: "Promotions");

            migrationBuilder.DropIndex(
                name: "IX_Promotions_HostId",
                table: "Promotions");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "Promotions");
        }
    }
}
