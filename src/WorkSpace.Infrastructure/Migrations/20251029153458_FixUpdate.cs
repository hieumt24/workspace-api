using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkSpaceRoomAmenities",
                table: "WorkSpaceRoomAmenities");

            migrationBuilder.DropIndex(
                name: "IX_WorkSpaceRoomAmenities_WorkSpaceRoomId",
                table: "WorkSpaceRoomAmenities");

            migrationBuilder.DropColumn(name: "WorkspaceId", table: "WorkSpaceRoomAmenities");
            migrationBuilder.DropColumn(name: "CreateUtc", table: "WorkSpaceRoomAmenities");
            migrationBuilder.DropColumn(name: "CreatedById", table: "WorkSpaceRoomAmenities");
            migrationBuilder.DropColumn(name: "IsAvailable", table: "WorkSpaceRoomAmenities");
            migrationBuilder.DropColumn(name: "LastModifiedById", table: "WorkSpaceRoomAmenities");
            migrationBuilder.DropColumn(name: "LastModifiedUtc", table: "WorkSpaceRoomAmenities");

            migrationBuilder.DropColumn(name: "Id", table: "WorkSpaceRoomAmenities");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "WorkSpaceRoomAmenities",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkSpaceRoomAmenities",
                table: "WorkSpaceRoomAmenities",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceRoomAmenities_WorkSpaceRoomId_AmenityId",
                table: "WorkSpaceRoomAmenities",
                columns: new[] { "WorkSpaceRoomId", "AmenityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkSpaceRoomAmenities",
                table: "WorkSpaceRoomAmenities");

            migrationBuilder.DropIndex(
                name: "IX_WorkSpaceRoomAmenities_WorkSpaceRoomId_AmenityId",
                table: "WorkSpaceRoomAmenities");

            migrationBuilder.DropColumn(name: "Id", table: "WorkSpaceRoomAmenities");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "WorkSpaceRoomAmenities",
                type: "int",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "WorkSpaceRoomAmenities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateUtc",
                table: "WorkSpaceRoomAmenities",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), TimeSpan.Zero));

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "WorkSpaceRoomAmenities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "WorkSpaceRoomAmenities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedById",
                table: "WorkSpaceRoomAmenities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModifiedUtc",
                table: "WorkSpaceRoomAmenities",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkSpaceRoomAmenities",
                table: "WorkSpaceRoomAmenities",
                columns: new[] { "WorkspaceId", "AmenityId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceRoomAmenities_WorkSpaceRoomId",
                table: "WorkSpaceRoomAmenities",
                column: "WorkSpaceRoomId");
        }
    }
}
