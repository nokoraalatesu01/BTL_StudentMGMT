using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeModelRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiangViens_DonVi_DonViId",
                table: "GiangViens");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHocs_DonVi_DonViId",
                table: "MonHocs");

            migrationBuilder.AlterColumn<string>(
                name: "Thu",
                table: "LichGiangDays",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhongHoc",
                table: "LichGiangDays",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HocKy",
                table: "LichGiangDays",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "LichGiangDays",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DiemDanhs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "DiemDanhs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GiangViens_DonVi_DonViId",
                table: "GiangViens",
                column: "DonViId",
                principalTable: "DonVi",
                principalColumn: "DonViId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MonHocs_DonVi_DonViId",
                table: "MonHocs",
                column: "DonViId",
                principalTable: "DonVi",
                principalColumn: "DonViId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiangViens_DonVi_DonViId",
                table: "GiangViens");

            migrationBuilder.DropForeignKey(
                name: "FK_MonHocs_DonVi_DonViId",
                table: "MonHocs");

            migrationBuilder.AlterColumn<string>(
                name: "Thu",
                table: "LichGiangDays",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "PhongHoc",
                table: "LichGiangDays",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "HocKy",
                table: "LichGiangDays",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "LichGiangDays",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DiemDanhs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "DiemDanhs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

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
    }
}
