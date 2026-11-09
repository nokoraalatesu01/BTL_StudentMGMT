using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleCaPerLopHocPhan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_LopHocPhan_SingleCa",
                table: "LopHocPhan",
                sql: "[TietBatDau] IN (1, 3, 5, 7, 9) AND [SoTiet] = 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LopHocPhan_SingleCa",
                table: "LopHocPhan");
        }
    }
}
