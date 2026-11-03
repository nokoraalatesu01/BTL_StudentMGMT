using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddHocLaiThiLaiCaiThienToDiem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DiemCuoiKyLan2",
                table: "Diems",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "Diems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HocKyId",
                table: "Diems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LanHoc",
                table: "Diems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "LanThi",
                table: "Diems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "LoaiHoc",
                table: "Diems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "HOC_LAN_DAU");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayCapNhat",
                table: "Diems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiCapNhat",
                table: "Diems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SuDungTinhTichLuy",
                table: "Diems",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "Diems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "DAT");

            migrationBuilder.AddColumn<string>(
                name: "LoaiDangKy",
                table: "DangKyHocPhan",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "HOC_LAN_DAU");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_HocKyId",
                table: "Diems",
                column: "HocKyId");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_SinhVienId_MonHocId_SuDungTinhTichLuy",
                table: "Diems",
                columns: new[] { "SinhVienId", "MonHocId", "SuDungTinhTichLuy" });

            migrationBuilder.AddForeignKey(
                name: "FK_Diems_HocKy_HocKyId",
                table: "Diems",
                column: "HocKyId",
                principalTable: "HocKy",
                principalColumn: "HocKyId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diems_HocKy_HocKyId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_Diems_HocKyId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_Diems_SinhVienId_MonHocId_SuDungTinhTichLuy",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "DiemCuoiKyLan2",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "HocKyId",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "LanHoc",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "LanThi",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "LoaiHoc",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "NgayCapNhat",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "NguoiCapNhat",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "SuDungTinhTichLuy",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "Diems");

            migrationBuilder.DropColumn(
                name: "LoaiDangKy",
                table: "DangKyHocPhan");
        }
    }
}
