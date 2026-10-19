using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFacultyNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [dbo].[Nganhs]
                SET [TenKhoa] = N'Công trình và Môi trường'
                WHERE [TenKhoa] IN
                    (N'Công trình', N'Kỹ thuật xây dựng', N'Môi trường và ATGT', N'Quản lý xây dựng');

                UPDATE [dbo].[Nganhs]
                SET [TenKhoa] = N'Kinh tế - Vận tải'
                WHERE [TenKhoa] = N'Vận tải - Kinh tế';

                UPDATE [dbo].[ChuongTrinhDaoTaos]
                SET [KhoaChuyenMon] = N'Công trình và Môi trường'
                WHERE [KhoaChuyenMon] IN
                    (N'Công trình', N'Kỹ thuật xây dựng', N'Môi trường và ATGT', N'Quản lý xây dựng');

                UPDATE [dbo].[ChuongTrinhDaoTaos]
                SET [KhoaChuyenMon] = N'Kinh tế - Vận tải'
                WHERE [KhoaChuyenMon] = N'Vận tải - Kinh tế';

                UPDATE [dbo].[ChuongTrinhDaoTaos]
                SET [KhoaQuanLy] = N'Trung tâm Đào tạo quốc tế'
                WHERE [KhoaQuanLy] = N'Đào tạo quốc tế';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [dbo].[Nganhs]
                SET [TenKhoa] = CASE
                    WHEN [MaNganh] IN (N'7580301', N'7580302') THEN N'Quản lý xây dựng'
                    WHEN [MaNganh] IN (N'7580201', N'7580101') THEN N'Kỹ thuật xây dựng'
                    WHEN [MaNganh] IN (N'7510104', N'7520320') THEN N'Môi trường và ATGT'
                    ELSE N'Công trình'
                END
                WHERE [TenKhoa] = N'Công trình và Môi trường'
                  AND [MaNganh] IN
                    (N'7580205', N'7580202', N'7580210', N'7580106', N'7580301',
                     N'7580302', N'7580201', N'7580101', N'7510104', N'7520320');

                UPDATE [dbo].[Nganhs]
                SET [TenKhoa] = N'Vận tải - Kinh tế'
                WHERE [TenKhoa] = N'Kinh tế - Vận tải';

                UPDATE [dbo].[ChuongTrinhDaoTaos]
                SET [KhoaChuyenMon] = CASE
                    WHEN [MaChuongTrinh] IN (N'IE5', N'IE6', N'LK-QLXD-BEDS') THEN N'Quản lý xây dựng'
                    WHEN [MaChuongTrinh] = N'IE1' THEN N'Kỹ thuật xây dựng'
                    ELSE N'Công trình'
                END
                WHERE [KhoaChuyenMon] = N'Công trình và Môi trường'
                  AND [MaChuongTrinh] IN
                    (N'IE5', N'IE6', N'IE9', N'IE10', N'IE2', N'IE1', N'IE0',
                     N'LK-QLXD-BEDS', N'LK-DSTC-DY');

                UPDATE [dbo].[ChuongTrinhDaoTaos]
                SET [KhoaChuyenMon] = N'Vận tải - Kinh tế'
                WHERE [KhoaChuyenMon] = N'Kinh tế - Vận tải';

                UPDATE [dbo].[ChuongTrinhDaoTaos]
                SET [KhoaQuanLy] = N'Đào tạo quốc tế'
                WHERE [KhoaQuanLy] = N'Trung tâm Đào tạo quốc tế';
                """);
        }
    }
}
