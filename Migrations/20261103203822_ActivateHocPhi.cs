using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class ActivateHocPhi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BieuPhiHocPhi",
                columns: table => new
                {
                    BieuPhiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChuongTrinhId = table.Column<int>(type: "int", nullable: true),
                    NamHocId = table.Column<int>(type: "int", nullable: false),
                    DonGiaTinChi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoaiTien = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "VND"),
                    HieuLucTu = table.Column<DateOnly>(type: "date", nullable: false),
                    HieuLucDen = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BieuPhiHocPhi", x => x.BieuPhiId);
                    table.ForeignKey(
                        name: "FK_BieuPhiHocPhi_ChuongTrinhDaoTaos_ChuongTrinhId",
                        column: x => x.ChuongTrinhId,
                        principalTable: "ChuongTrinhDaoTaos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BieuPhiHocPhi_NamHoc_NamHocId",
                        column: x => x.NamHocId,
                        principalTable: "NamHoc",
                        principalColumn: "NamHocId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonHocPhi",
                columns: table => new
                {
                    HoaDonId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoHoaDon = table.Column<string>(type: "varchar(40)", unicode: false, maxLength: 40, nullable: false),
                    SinhVienId = table.Column<int>(type: "int", nullable: false),
                    HocKyId = table.Column<int>(type: "int", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    HanThanhToan = table.Column<DateOnly>(type: "date", nullable: true),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "ISSUED"),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonHocPhi", x => x.HoaDonId);
                    table.ForeignKey(
                        name: "FK_HoaDonHocPhi_HocKy_HocKyId",
                        column: x => x.HocKyId,
                        principalTable: "HocKy",
                        principalColumn: "HocKyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDonHocPhi_SinhViens_SinhVienId",
                        column: x => x.SinhVienId,
                        principalTable: "SinhViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDonHocPhi",
                columns: table => new
                {
                    ChiTietHoaDonId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoaDonId = table.Column<long>(type: "bigint", nullable: false),
                    LoaiKhoanThu = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoTienGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDonHocPhi", x => x.ChiTietHoaDonId);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDonHocPhi_HoaDonHocPhi_HoaDonId",
                        column: x => x.HoaDonId,
                        principalTable: "HoaDonHocPhi",
                        principalColumn: "HoaDonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToan",
                columns: table => new
                {
                    ThanhToanId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoaDonId = table.Column<long>(type: "bigint", nullable: false),
                    MaGiaoDich = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThoiGianThanhToan = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    PhuongThuc = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    NguonThanhToan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AnhMinhChung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToan", x => x.ThanhToanId);
                    table.ForeignKey(
                        name: "FK_ThanhToan_HoaDonHocPhi_HoaDonId",
                        column: x => x.HoaDonId,
                        principalTable: "HoaDonHocPhi",
                        principalColumn: "HoaDonId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BieuPhiHocPhi_ChuongTrinhId",
                table: "BieuPhiHocPhi",
                column: "ChuongTrinhId");

            migrationBuilder.CreateIndex(
                name: "IX_BieuPhiHocPhi_NamHocId",
                table: "BieuPhiHocPhi",
                column: "NamHocId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDonHocPhi_HoaDonId",
                table: "ChiTietHoaDonHocPhi",
                column: "HoaDonId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonHocPhi_HocKyId",
                table: "HoaDonHocPhi",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "UQ_HoaDon_So",
                table: "HoaDonHocPhi",
                column: "SoHoaDon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_HoaDon_SV_HocKy",
                table: "HoaDonHocPhi",
                columns: new[] { "SinhVienId", "HocKyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_HD_TrangThai",
                table: "ThanhToan",
                columns: new[] { "HoaDonId", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "UQ_ThanhToan_Idempotency",
                table: "ThanhToan",
                column: "IdempotencyKey",
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_ThanhToan_MaGiaoDich",
                table: "ThanhToan",
                column: "MaGiaoDich",
                unique: true,
                filter: "[MaGiaoDich] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BieuPhiHocPhi");

            migrationBuilder.DropTable(
                name: "ChiTietHoaDonHocPhi");

            migrationBuilder.DropTable(
                name: "ThanhToan");

            migrationBuilder.DropTable(
                name: "HoaDonHocPhi");
        }
    }
}
