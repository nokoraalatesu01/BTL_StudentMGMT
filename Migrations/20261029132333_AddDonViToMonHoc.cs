using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddDonViToMonHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DonVi",
                newName: "DonViId");

            migrationBuilder.AddColumn<int>(
                name: "DonViId",
                table: "MonHocs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DonViId",
                table: "GiangViens",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenDonVi",
                table: "DonVi",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LoaiDonVi",
                table: "DonVi",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "DonVi",
                type: "datetime2(0)",
                precision: 0,
                nullable: false,
                defaultValueSql: "(sysutcdatetime())")
                .Annotation("Relational:DefaultConstraintName", "DF_DonVi_CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "DienThoai",
                table: "DonVi",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DonViChaId",
                table: "DonVi",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DonVi",
                type: "varchar(254)",
                unicode: false,
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaDonVi",
                table: "DonVi",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVer",
                table: "DonVi",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "DonVi",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVE")
                .Annotation("Relational:DefaultConstraintName", "DF_DonVi_TrangThai");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DonVi",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaChuongTrinh",
                table: "ChuongTrinhDaoTaos",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DiemDanhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    LopId = table.Column<int>(type: "int", nullable: false),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    NgayDiemDanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemDanhs_Lops_LopId",
                        column: x => x.LopId,
                        principalTable: "Lops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiemDanhs_MonHocs_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiemDanhs_SinhViens_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaHocTap",
                columns: table => new
                {
                    DangKyId = table.Column<long>(type: "bigint", nullable: false),
                    DiemTongKet = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    DiemChu = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: true),
                    DiemHe4 = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    Dat = table.Column<bool>(type: "bit", nullable: true),
                    TinChiTinhTichLuy = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayChot = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NguoiChot = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PhienBan = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                        .Annotation("Relational:DefaultConstraintName", "DF_KQ_PhienBan")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaHocTap", x => x.DangKyId);
                });

            migrationBuilder.CreateTable(
                name: "KhoaTuyenSinh",
                columns: table => new
                {
                    KhoaTuyenSinhId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhoa = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NamTuyenSinh = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaTuyenSinh", x => x.KhoaTuyenSinhId);
                });

            migrationBuilder.CreateTable(
                name: "LichGiangDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiangVienId = table.Column<int>(type: "int", nullable: true),
                    MonHocId = table.Column<int>(type: "int", nullable: false),
                    LopId = table.Column<int>(type: "int", nullable: false),
                    Thu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TietBatDau = table.Column<int>(type: "int", nullable: false),
                    SoTiet = table.Column<int>(type: "int", nullable: false),
                    PhongHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HocKy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichGiangDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichGiangDays_GiangViens_GiangVienId",
                        column: x => x.GiangVienId,
                        principalTable: "GiangViens",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LichGiangDays_Lops_LopId",
                        column: x => x.LopId,
                        principalTable: "Lops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichGiangDays_MonHocs_MonHocId",
                        column: x => x.MonHocId,
                        principalTable: "MonHocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NamHoc",
                columns: table => new
                {
                    NamHocId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNamHoc = table.Column<string>(type: "varchar(9)", unicode: false, maxLength: 9, nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "PLANNED")
                        .Annotation("Relational:DefaultConstraintName", "DF_NamHoc_TrangThai")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamHoc", x => x.NamHocId);
                });

            migrationBuilder.CreateTable(
                name: "PhongHoc",
                columns: table => new
                {
                    PhongHocId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhong = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TenPhong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CoSo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SucChua = table.Column<short>(type: "smallint", nullable: false),
                    LoaiPhong = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "LY_THUYET")
                        .Annotation("Relational:DefaultConstraintName", "DF_Phong_Loai"),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "ACTIVE")
                        .Annotation("Relational:DefaultConstraintName", "DF_Phong_TrangThai")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongHoc", x => x.PhongHocId);
                });

            migrationBuilder.CreateTable(
                name: "ThanhPhanDiem",
                columns: table => new
                {
                    ThanhPhanDiemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LopHocPhanId = table.Column<int>(type: "int", nullable: false),
                    MaThanhPhan = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TenThanhPhan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrongSo = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DiemToiDa = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 10m)
                        .Annotation("Relational:DefaultConstraintName", "DF_TPD_DiemMax"),
                    BatBuoc = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_TPD_BatBuoc")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhPhanDiem", x => x.ThanhPhanDiemId);
                });

            migrationBuilder.CreateTable(
                name: "ChuongTrinhHocBong",
                columns: table => new
                {
                    HocBongId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHocBong = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    TenHocBong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    SoTienToiDa = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TieuChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "OPEN")
                        .Annotation("Relational:DefaultConstraintName", "DF_HocBong_TrangThai")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhHocBong", x => x.HocBongId);
                    table.ForeignKey(
                        name: "FK_HocBong_NamHoc",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "NamHocId");
                });

            migrationBuilder.CreateTable(
                name: "HocKy",
                columns: table => new
                {
                    HocKyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    MaHocKy = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TenHocKy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    HanDangKyTu = table.Column<DateOnly>(type: "date", nullable: true),
                    HanDangKyDen = table.Column<DateOnly>(type: "date", nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "PLANNED")
                        .Annotation("Relational:DefaultConstraintName", "DF_HocKy_TrangThai")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocKy", x => x.HocKyId);
                    table.ForeignKey(
                        name: "FK_HocKy_NamHoc",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "NamHocId");
                });

            migrationBuilder.CreateTable(
                name: "LichHoc",
                columns: table => new
                {
                    LichHocId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LopHocPhanId = table.Column<int>(type: "int", nullable: false),
                    PhongHocId = table.Column<int>(type: "int", nullable: false),
                    Thu = table.Column<byte>(type: "tinyint", nullable: false),
                    TietBatDau = table.Column<byte>(type: "tinyint", nullable: false),
                    SoTiet = table.Column<byte>(type: "tinyint", nullable: false),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: false),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: false),
                    GioBatDau = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: true),
                    GioKetThuc = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHoc", x => x.LichHocId);
                    table.ForeignKey(
                        name: "FK_LichHoc_Phong",
                        column: x => x.PhongHocId,
                        principalTable: "PhongHoc",
                        principalColumn: "PhongHocId");
                });

            migrationBuilder.CreateTable(
                name: "DiemThanhPhan",
                columns: table => new
                {
                    DangKyId = table.Column<long>(type: "bigint", nullable: false),
                    ThanhPhanDiemId = table.Column<int>(type: "int", nullable: false),
                    Diem = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NguoiNhap = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NgayNhap = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemThanhPhan", x => new { x.DangKyId, x.ThanhPhanDiemId });
                    table.ForeignKey(
                        name: "FK_DTP_TPD",
                        column: x => x.ThanhPhanDiemId,
                        principalTable: "ThanhPhanDiem",
                        principalColumn: "ThanhPhanDiemId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonHocs_DonViId",
                table: "MonHocs",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_DonViId",
                table: "GiangViens",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_DonViChaId",
                table: "DonVi",
                column: "DonViChaId");

            migrationBuilder.CreateIndex(
                name: "UQ_DonVi_Ma",
                table: "DonVi",
                column: "MaDonVi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhHocBong_NamHocId",
                table: "ChuongTrinhHocBong",
                column: "NamHocId");

            migrationBuilder.CreateIndex(
                name: "UQ_HocBong_Ma",
                table: "ChuongTrinhHocBong",
                column: "MaHocBong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhs_LopId",
                table: "DiemDanhs",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhs_MonHocId",
                table: "DiemDanhs",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanhs_SinhVienId",
                table: "DiemDanhs",
                column: "SinhVienId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemThanhPhan_ThanhPhanDiemId",
                table: "DiemThanhPhan",
                column: "ThanhPhanDiemId");

            migrationBuilder.CreateIndex(
                name: "UQ_HocKy",
                table: "HocKy",
                columns: new[] { "NamHocId", "MaHocKy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_KhoaTuyenSinh_Ma",
                table: "KhoaTuyenSinh",
                column: "MaKhoa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichGiangDays_GiangVienId",
                table: "LichGiangDays",
                column: "GiangVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LichGiangDays_LopId",
                table: "LichGiangDays",
                column: "LopId");

            migrationBuilder.CreateIndex(
                name: "IX_LichGiangDays_MonHocId",
                table: "LichGiangDays",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHoc_PhongHocId",
                table: "LichHoc",
                column: "PhongHocId");

            migrationBuilder.CreateIndex(
                name: "UQ_NamHoc_Ma",
                table: "NamHoc",
                column: "MaNamHoc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PhongHoc_Ma",
                table: "PhongHoc",
                column: "MaPhong",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DonVi_Cha",
                table: "DonVi",
                column: "DonViChaId",
                principalTable: "DonVi",
                principalColumn: "DonViId");

            migrationBuilder.AddForeignKey(
                name: "FK_GiangViens_DonVi_DonViId",
                table: "GiangViens",
                column: "DonViId",
                principalTable: "DonVi",
                principalColumn: "DonViId");

            migrationBuilder.AddForeignKey(
                name: "FK_MonHocs_DonVi_DonViId",
                table: "MonHocs",
                column: "DonViId",
                principalTable: "DonVi",
                principalColumn: "DonViId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonVi_Cha",
                table: "DonVi");

            migrationBuilder.DropForeignKey(
                name: "FK_GiangViens_DonVi_DonViId",
                table: "GiangViens");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHocs_DonVi_DonViId",
                table: "MonHocs");

            migrationBuilder.DropTable(
                name: "ChuongTrinhHocBong");

            migrationBuilder.DropTable(
                name: "DiemDanhs");

            migrationBuilder.DropTable(
                name: "DiemThanhPhan");

            migrationBuilder.DropTable(
                name: "HocKy");

            migrationBuilder.DropTable(
                name: "KetQuaHocTap");

            migrationBuilder.DropTable(
                name: "KhoaTuyenSinh");

            migrationBuilder.DropTable(
                name: "LichGiangDays");

            migrationBuilder.DropTable(
                name: "LichHoc");

            migrationBuilder.DropTable(
                name: "ThanhPhanDiem");

            migrationBuilder.DropTable(
                name: "NamHoc");

            migrationBuilder.DropTable(
                name: "PhongHoc");

            migrationBuilder.DropIndex(
                name: "IX_MonHocs_DonViId",
                table: "MonHocs");

            migrationBuilder.DropIndex(
                name: "IX_GiangViens_DonViId",
                table: "GiangViens");

            migrationBuilder.DropIndex(
                name: "IX_DonVi_DonViChaId",
                table: "DonVi");

            migrationBuilder.DropIndex(
                name: "UQ_DonVi_Ma",
                table: "DonVi");

            migrationBuilder.DropColumn(
                name: "DonViId",
                table: "MonHocs");

            migrationBuilder.DropColumn(
                name: "DonViId",
                table: "GiangViens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DonVi")
                .Annotation("Relational:DefaultConstraintName", "DF_DonVi_CreatedAt");

            migrationBuilder.DropColumn(
                name: "DienThoai",
                table: "DonVi");

            migrationBuilder.DropColumn(
                name: "DonViChaId",
                table: "DonVi");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DonVi");

            migrationBuilder.DropColumn(
                name: "MaDonVi",
                table: "DonVi");

            migrationBuilder.DropColumn(
                name: "RowVer",
                table: "DonVi");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "DonVi")
                .Annotation("Relational:DefaultConstraintName", "DF_DonVi_TrangThai");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DonVi");

            migrationBuilder.RenameColumn(
                name: "DonViId",
                table: "DonVi",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "TenDonVi",
                table: "DonVi",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LoaiDonVi",
                table: "DonVi",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "MaChuongTrinh",
                table: "ChuongTrinhDaoTaos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
