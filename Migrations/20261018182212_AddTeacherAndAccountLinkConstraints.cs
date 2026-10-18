using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_StudentMGMT.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherAndAccountLinkConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SinhViens_UserId",
                table: "SinhViens");

            migrationBuilder.DropIndex(
                name: "IX_GiangViens_UserId",
                table: "GiangViens");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "GiangViens",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaGiangVien",
                table: "GiangViens",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "GiangViens",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "GiangViens",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[MonHocs]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[MonHocs] (
                        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_MonHocs] PRIMARY KEY,
                        [MaMon] nvarchar(50) NOT NULL,
                        [TenMon] nvarchar(255) NOT NULL,
                        [SoTinChi] int NOT NULL
                    );
                END;

                IF OBJECT_ID(N'[dbo].[Diems]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Diems] (
                        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Diems] PRIMARY KEY,
                        [SinhVienId] int NOT NULL,
                        [MonHocId] int NOT NULL,
                        [DiemChuyenCan] float NOT NULL,
                        [DiemGiuaKy] float NOT NULL,
                        [DiemCuoiKy] float NOT NULL
                    );
                END;

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Diems_MonHocs_MonHocId')
                    ALTER TABLE [dbo].[Diems] WITH CHECK ADD CONSTRAINT [FK_Diems_MonHocs_MonHocId]
                    FOREIGN KEY ([MonHocId]) REFERENCES [dbo].[MonHocs] ([Id]) ON DELETE NO ACTION;

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Diems_SinhViens_SinhVienId')
                    ALTER TABLE [dbo].[Diems] WITH CHECK ADD CONSTRAINT [FK_Diems_SinhViens_SinhVienId]
                    FOREIGN KEY ([SinhVienId]) REFERENCES [dbo].[SinhViens] ([Id]) ON DELETE NO ACTION;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_SinhViens_UserId",
                table: "SinhViens",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_MaGiangVien",
                table: "GiangViens",
                column: "MaGiangVien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_UserId",
                table: "GiangViens",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_MonHocId",
                table: "Diems",
                column: "MonHocId");

            migrationBuilder.CreateIndex(
                name: "IX_Diems_SinhVienId",
                table: "Diems",
                column: "SinhVienId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Diems_MonHocs_MonHocId')
                    ALTER TABLE [dbo].[Diems] DROP CONSTRAINT [FK_Diems_MonHocs_MonHocId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Diems_SinhViens_SinhVienId')
                    ALTER TABLE [dbo].[Diems] DROP CONSTRAINT [FK_Diems_SinhViens_SinhVienId];
                """);

            migrationBuilder.DropIndex(
                name: "IX_Diems_MonHocId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_Diems_SinhVienId",
                table: "Diems");

            migrationBuilder.DropIndex(
                name: "IX_SinhViens_UserId",
                table: "SinhViens");

            migrationBuilder.DropIndex(
                name: "IX_GiangViens_MaGiangVien",
                table: "GiangViens");

            migrationBuilder.DropIndex(
                name: "IX_GiangViens_UserId",
                table: "GiangViens");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaGiangVien",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SinhViens_UserId",
                table: "SinhViens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_UserId",
                table: "GiangViens",
                column: "UserId");
        }
    }
}
