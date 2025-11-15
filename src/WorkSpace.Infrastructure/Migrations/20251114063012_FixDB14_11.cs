using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDB14_11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa bảng cũ (Payments)
            migrationBuilder.DropTable(
                name: "Payments");

            // Bước 2: Thêm cột mới PaymentMethodID vào Bookings
            // Cột này được tạo với nullable: false và defaultValue: 0
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodID",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Bước 3: Tạo bảng PaymentMethods mới
            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    PaymentMethodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMethodName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.PaymentMethodID);
                });

            // --- BƯỚC SỬA LỖI KHÓA NGOẠI: Bắt đầu ---

            // Bước 4A: Chèn dữ liệu mặc định vào bảng PaymentMethods (ID=1)
            // Cần phải có ít nhất một bản ghi để các bản ghi cũ trong Bookings tham chiếu đến
            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "PaymentMethodID", "PaymentMethodName", "PaymentCost" },
                values: new object[,]
                {
                    // LƯU Ý: Đây là phương thức thanh toán mặc định/ban đầu để tránh lỗi FK
                    { 1, "Default/Initial Payment Method", 0m }
                });

            // Bước 4B: Cập nhật dữ liệu cũ trong bảng Bookings
            // Cột PaymentMethodID trên các hàng Bookings đã tồn tại được tạo với giá trị 0 
            // do defaultValue: 0 ở Bước 2.
            // Lệnh này cập nhật tất cả các hàng đó thành 1 (giá trị vừa được InsertData) 
            // để thỏa mãn ràng buộc Khóa ngoại.
            migrationBuilder.Sql("UPDATE Bookings SET PaymentMethodID = 1 WHERE PaymentMethodID = 0");

            // --- BƯỚC SỬA LỖI KHÓA NGOẠI: Kết thúc ---

            // Bước 5: Tạo Index và Khóa ngoại như cũ
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymentMethodID",
                table: "Bookings",
                column: "PaymentMethodID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_PaymentMethods_PaymentMethodID",
                table: "Bookings",
                column: "PaymentMethodID",
                principalTable: "PaymentMethods",
                principalColumn: "PaymentMethodID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Các lệnh Down() không cần thay đổi
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_PaymentMethods_PaymentMethodID",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PaymentMethodID",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PaymentMethodID",
                table: "Bookings");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PaymentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId",
                unique: true);
        }
    }
}