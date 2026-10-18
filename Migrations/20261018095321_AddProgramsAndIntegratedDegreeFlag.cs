using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramsAndIntegratedDegreeFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TichHopCuNhanKySu",
                table: "Nganhs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChuongTrinhDaoTaos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuongTrinh = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TenChuongTrinh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LoaiChuongTrinh = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    KhoaQuanLy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaNganhGoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HinhThucLienKet = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApDungTuKhoa = table.Column<int>(type: "int", nullable: true),
                    TichHopCuNhanKySu = table.Column<bool>(type: "bit", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NganhId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuongTrinhDaoTaos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuongTrinhDaoTaos_Nganhs_NganhId",
                        column: x => x.NganhId,
                        principalTable: "Nganhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTaos_MaChuongTrinh",
                table: "ChuongTrinhDaoTaos",
                column: "MaChuongTrinh",
                unique: true,
                filter: "[MaChuongTrinh] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChuongTrinhDaoTaos_NganhId",
                table: "ChuongTrinhDaoTaos",
                column: "NganhId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuongTrinhDaoTaos");

            migrationBuilder.DropColumn(
                name: "TichHopCuNhanKySu",
                table: "Nganhs");
        }
    }
}
