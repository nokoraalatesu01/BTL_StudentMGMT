using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeDataConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diems_SinhViens_SinhVienId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_Diems_SinhVienId",
                table: "Diems");

            migrationBuilder.CreateIndex(
                name: "IX_MonHocs_MaMon",
                table: "MonHocs",
                column: "MaMon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diems_SinhVienId_MonHocId",
                table: "Diems",
                columns: new[] { "SinhVienId", "MonHocId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Diems_SinhViens_SinhVienId",
                table: "Diems",
                column: "SinhVienId",
                principalTable: "SinhViens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Diems_SinhViens_SinhVienId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_MonHocs_MaMon",
                table: "MonHocs");

            migrationBuilder.DropIndex(
                name: "IX_Diems_SinhVienId_MonHocId",
                table: "Diems");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_SinhVienId",
                table: "Diems",
                column: "SinhVienId");

            migrationBuilder.AddForeignKey(
                name: "FK_Diems_SinhViens_SinhVienId",
                table: "Diems",
                column: "SinhVienId",
                principalTable: "SinhViens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
