using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddLopHocPhanAndDangKyHocPhan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiemDanhs_Lops_LopId",
                table: "DiemDanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemDanhs_MonHocs_MonHocId",
                table: "DiemDanhs");

            migrationBuilder.DropIndex(
                name: "IX_Diems_SinhVienId_MonHocId",
                table: "Diems");

            migrationBuilder.AddColumn<int>(
                name: "LopHocPhanId",
                table: "Diems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LopId",
                table: "DiemDanhs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LopHocPhanId",
                table: "DiemDanhs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LopHocPhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLopHocPhan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TenLopHocPhan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: true),
                    GiangVienId = table.Column<int>(type: "int", nullable: true),
                    Nhom = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "01"),
                    SiSoToiDa = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "OPEN"),
                    Thu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TietBatDau = table.Column<int>(type: "int", nullable: false),
                    SoTiet = table.Column<int>(type: "int", nullable: false),
                    PhongHoc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHocPhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LopHocPhan_GiangViens_GiangVienId",
                        column: x => x.GiangVienId,
                        principalTable: "GiangViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LopHocPhan_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "HocKyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LopHocPhan_MonHocs_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKyHocPhan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    LopHocPhanId = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ENROLLED"),
                    LanHoc = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    GhiChu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyHocPhan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangKyHocPhan_LopHocPhan_LopHocPhanId",
                        column: x => x.LopHocPhanId,
                        principalTable: "LopHocPhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DangKyHocPhan_SinhViens_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diems_LopHocPhanId",
                table: "Diems",
                column: "LopHocPhanId");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_SinhVienId_LopHocPhanId",
                table: "Diems",
                columns: new[] { "SinhVienId", "LopHocPhanId" },
                unique: true,
                filter: "[LopHocPhanId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhs_LopHocPhanId",
                table: "DiemDanhs",
                column: "LopHocPhanId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyHocPhan_LopHocPhanId",
                table: "DangKyHocPhan",
                column: "LopHocPhanId");

            migrationBuilder.CreateIndex(
                name: "UQ_DK_SV_LHP",
                table: "DangKyHocPhan",
                columns: new[] { "SinhVienId", "LopHocPhanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_GiangVienId",
                table: "LopHocPhan",
                column: "GiangVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_HocKyId",
                table: "LopHocPhan",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_MaLopHocPhan",
                table: "LopHocPhan",
                column: "MaLopHocPhan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHocPhan_MonHocId",
                table: "LopHocPhan",
                column: "MonHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanhs_LopHocPhan_LopHocPhanId",
                table: "DiemDanhs",
                column: "LopHocPhanId",
                principalTable: "LopHocPhan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanhs_Lops_LopId",
                table: "DiemDanhs",
                column: "LopId",
                principalTable: "Lops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanhs_MonHocs_MonHocId",
                table: "DiemDanhs",
                column: "MonHocId",
                principalTable: "MonHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Diems_LopHocPhan_LopHocPhanId",
                table: "Diems",
                column: "LopHocPhanId",
                principalTable: "LopHocPhan",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiemDanhs_LopHocPhan_LopHocPhanId",
                table: "DiemDanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemDanhs_Lops_LopId",
                table: "DiemDanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_DiemDanhs_MonHocs_MonHocId",
                table: "DiemDanhs");

            migrationBuilder.DropForeignKey(
                name: "FK_Diems_LopHocPhan_LopHocPhanId",
                table: "Diems");

            migrationBuilder.DropTable(
                name: "DangKyHocPhan");

            migrationBuilder.DropTable(
                name: "LopHocPhan");

            migrationBuilder.DropIndex(
                name: "IX_Diems_LopHocPhanId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_Diems_SinhVienId_LopHocPhanId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_DiemDanhs_LopHocPhanId",
                table: "DiemDanhs");

            migrationBuilder.DropColumn(
                name: "LopHocPhanId",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "LopHocPhanId",
                table: "DiemDanhs");

            migrationBuilder.AlterColumn<int>(
                name: "LopId",
                table: "DiemDanhs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diems_SinhVienId_MonHocId",
                table: "Diems",
                columns: new[] { "SinhVienId", "MonHocId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanhs_Lops_LopId",
                table: "DiemDanhs",
                column: "LopId",
                principalTable: "Lops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiemDanhs_MonHocs_MonHocId",
                table: "DiemDanhs",
                column: "MonHocId",
                principalTable: "MonHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
