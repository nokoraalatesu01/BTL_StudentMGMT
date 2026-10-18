using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramProfessionalFaculty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KhoaChuyenMon",
                table: "ChuongTrinhDaoTaos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM [ChuongTrinhDaoTaos] WHERE [MaChuongTrinh] = N'CLC-QLXDCTGT')
                BEGIN
                    IF EXISTS (SELECT 1 FROM [ChuongTrinhDaoTaos] WHERE [MaChuongTrinh] = N'IE6')
                    BEGIN
                        UPDATE [currentProgram]
                        SET [currentProgram].[NganhId] = COALESCE([currentProgram].[NganhId], [legacyProgram].[NganhId]),
                            [currentProgram].[TenChuongTrinh] = COALESCE([currentProgram].[TenChuongTrinh], [legacyProgram].[TenChuongTrinh]),
                            [currentProgram].[LoaiChuongTrinh] = COALESCE([currentProgram].[LoaiChuongTrinh], [legacyProgram].[LoaiChuongTrinh]),
                            [currentProgram].[KhoaQuanLy] = COALESCE([currentProgram].[KhoaQuanLy], [legacyProgram].[KhoaQuanLy]),
                            [currentProgram].[KhoaChuyenMon] = COALESCE([currentProgram].[KhoaChuyenMon], [legacyProgram].[KhoaChuyenMon]),
                            [currentProgram].[MaNganhGoc] = COALESCE([currentProgram].[MaNganhGoc], [legacyProgram].[MaNganhGoc]),
                            [currentProgram].[HinhThucLienKet] = COALESCE([currentProgram].[HinhThucLienKet], [legacyProgram].[HinhThucLienKet]),
                            [currentProgram].[ApDungTuKhoa] = COALESCE([currentProgram].[ApDungTuKhoa], [legacyProgram].[ApDungTuKhoa]),
                            [currentProgram].[TichHopCuNhanKySu] = CASE
                                WHEN [currentProgram].[TichHopCuNhanKySu] = 1 OR [legacyProgram].[TichHopCuNhanKySu] = 1 THEN 1
                                ELSE 0
                            END,
                            [currentProgram].[GhiChu] = CASE
                                WHEN NULLIF([currentProgram].[GhiChu], N'') IS NULL THEN [legacyProgram].[GhiChu]
                                WHEN NULLIF([legacyProgram].[GhiChu], N'') IS NULL THEN [currentProgram].[GhiChu]
                                ELSE CONCAT([currentProgram].[GhiChu], N' | ', [legacyProgram].[GhiChu])
                            END
                        FROM [ChuongTrinhDaoTaos] AS [currentProgram]
                        INNER JOIN [ChuongTrinhDaoTaos] AS [legacyProgram]
                            ON [legacyProgram].[MaChuongTrinh] = N'CLC-QLXDCTGT'
                        WHERE [currentProgram].[MaChuongTrinh] = N'IE6';

                        DELETE FROM [ChuongTrinhDaoTaos]
                        WHERE [MaChuongTrinh] = N'CLC-QLXDCTGT';
                    END
                    ELSE
                    BEGIN
                        UPDATE [ChuongTrinhDaoTaos]
                        SET [MaChuongTrinh] = N'IE6'
                        WHERE [MaChuongTrinh] = N'CLC-QLXDCTGT';
                    END
                END;

                UPDATE [program]
                SET [program].[MaNganhGoc] = N'7580302',
                    [program].[NganhId] = [major].[Id]
                FROM [ChuongTrinhDaoTaos] AS [program]
                LEFT JOIN [Nganhs] AS [major] ON [major].[MaNganh] = N'7580302'
                WHERE [program].[MaChuongTrinh] = N'IE6';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KhoaChuyenMon",
                table: "ChuongTrinhDaoTaos");
        }
    }
}
