using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class GuardDuplicatePendingPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UQ_ThanhToan_HD_Pending",
                table: "ThanhToan",
                column: "HoaDonId",
                unique: true,
                filter: "[TrangThai] = 'PENDING'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ThanhToan_HD_Pending",
                table: "ThanhToan");
        }
    }
}
