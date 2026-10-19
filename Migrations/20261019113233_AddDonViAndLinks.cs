using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddDonViAndLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonViId",
                table: "Nganhs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChuongTrinhDaoTaoId",
                table: "Lops",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DonViChuyenMonId",
                table: "ChuongTrinhDaoTaos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DonViQuanLyId",
                table: "ChuongTrinhDaoTaos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DonVi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDonVi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiDonVi = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonVi", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nganhs_DonViId",
                table: "Nganhs",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_Lops_ChuongTrinhDaoTaoId",
                table: "Lops",
                column: "ChuongTrinhDaoTaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTaos_DonViChuyenMonId",
                table: "ChuongTrinhDaoTaos",
                column: "DonViChuyenMonId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTaos_DonViQuanLyId",
                table: "ChuongTrinhDaoTaos",
                column: "DonViQuanLyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChuongTrinhDaoTaos_DonVi_DonViChuyenMonId",
                table: "ChuongTrinhDaoTaos",
                column: "DonViChuyenMonId",
                principalTable: "DonVi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChuongTrinhDaoTaos_DonVi_DonViQuanLyId",
                table: "ChuongTrinhDaoTaos",
                column: "DonViQuanLyId",
                principalTable: "DonVi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lops_ChuongTrinhDaoTaos_ChuongTrinhDaoTaoId",
                table: "Lops",
                column: "ChuongTrinhDaoTaoId",
                principalTable: "ChuongTrinhDaoTaos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Nganhs_DonVi_DonViId",
                table: "Nganhs",
                column: "DonViId",
                principalTable: "DonVi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChuongTrinhDaoTaos_DonVi_DonViChuyenMonId",
                table: "ChuongTrinhDaoTaos");

            migrationBuilder.DropForeignKey(
                name: "FK_ChuongTrinhDaoTaos_DonVi_DonViQuanLyId",
                table: "ChuongTrinhDaoTaos");

            migrationBuilder.DropForeignKey(
                name: "FK_Lops_ChuongTrinhDaoTaos_ChuongTrinhDaoTaoId",
                table: "Lops");

            migrationBuilder.DropForeignKey(
                name: "FK_Nganhs_DonVi_DonViId",
                table: "Nganhs");

            migrationBuilder.DropTable(
                name: "DonVi");

            migrationBuilder.DropIndex(
                name: "IX_Nganhs_DonViId",
                table: "Nganhs");

            migrationBuilder.DropIndex(
                name: "IX_Lops_ChuongTrinhDaoTaoId",
                table: "Lops");

            migrationBuilder.DropIndex(
                name: "IX_ChuongTrinhDaoTaos_DonViChuyenMonId",
                table: "ChuongTrinhDaoTaos");

            migrationBuilder.DropIndex(
                name: "IX_ChuongTrinhDaoTaos_DonViQuanLyId",
                table: "ChuongTrinhDaoTaos");

            migrationBuilder.DropColumn(
                name: "DonViId",
                table: "Nganhs");

            migrationBuilder.DropColumn(
                name: "ChuongTrinhDaoTaoId",
                table: "Lops");

            migrationBuilder.DropColumn(
                name: "DonViChuyenMonId",
                table: "ChuongTrinhDaoTaos");

            migrationBuilder.DropColumn(
                name: "DonViQuanLyId",
                table: "ChuongTrinhDaoTaos");
        }
    }
}
