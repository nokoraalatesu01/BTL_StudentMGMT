using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddFacultyAndClassConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lops_Nganhs_NganhId",
                table: "Lops");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhViens_Lops_LopId",
                table: "SinhViens");

            migrationBuilder.DropIndex(
                name: "IX_Lops_NganhId",
                table: "Lops");

            migrationBuilder.AlterColumn<string>(
                name: "TenNganh",
                table: "Nganhs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaNganh",
                table: "Nganhs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "TenKhoa",
                table: "Nganhs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "TenLop",
                table: "Lops",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Nganhs_MaNganh",
                table: "Nganhs",
                column: "MaNganh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lops_NganhId_TenLop",
                table: "Lops",
                columns: new[] { "NganhId", "TenLop" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lops_Nganhs_NganhId",
                table: "Lops",
                column: "NganhId",
                principalTable: "Nganhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhViens_Lops_LopId",
                table: "SinhViens",
                column: "LopId",
                principalTable: "Lops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lops_Nganhs_NganhId",
                table: "Lops");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhViens_Lops_LopId",
                table: "SinhViens");

            migrationBuilder.DropIndex(
                name: "IX_Nganhs_MaNganh",
                table: "Nganhs");

            migrationBuilder.DropIndex(
                name: "IX_Lops_NganhId_TenLop",
                table: "Lops");

            migrationBuilder.DropColumn(
                name: "TenKhoa",
                table: "Nganhs");

            migrationBuilder.AlterColumn<string>(
                name: "TenNganh",
                table: "Nganhs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "MaNganh",
                table: "Nganhs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TenLop",
                table: "Lops",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.CreateIndex(
                name: "IX_Lops_NganhId",
                table: "Lops",
                column: "NganhId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lops_Nganhs_NganhId",
                table: "Lops",
                column: "NganhId",
                principalTable: "Nganhs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhViens_Lops_LopId",
                table: "SinhViens",
                column: "LopId",
                principalTable: "Lops",
                principalColumn: "Id");
        }
    }
}
