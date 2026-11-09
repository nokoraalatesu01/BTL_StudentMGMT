-- =========================================================================
-- SCRIPT CHẠY TAY THỦ CÔNG: PHASE 1 - CHUẨN HÓA ĐIỂM DANH & RÀNG BUỘC UNIQUE
-- Khuyến nghị: Thực hiện SAO LƯU (BACKUP) cơ sở dữ liệu trước khi chạy script này!
-- Script này được thiết kế IDEMPOTENT (có thể chạy an toàn nhiều lần).
-- =========================================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'>> Bắt đầu thực thi script chuẩn hóa Phase 1...';

-- 1. Chuẩn hóa NgayDiemDanh về dạng ngày (Date)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[DiemDanhs]') AND name = N'NgayDiemDanh')
BEGIN
    PRINT N'1. Chuẩn hóa NgayDiemDanh cắt bỏ giờ phút...';
    UPDATE [DiemDanhs] SET [NgayDiemDanh] = CAST([NgayDiemDanh] AS DATE);
END;

-- 2. Chuẩn hóa mã trạng thái cũ sang tiếng Anh chuẩn hóa (PRESENT, ABSENT, EXCUSED, LATE)
PRINT N'2. Chuẩn hóa trạng thái điểm danh sang mã chuẩn quốc tế...';
DECLARE @CoMat NVARCHAR(50) = N'C' + NCHAR(243) + N' m' + NCHAR(7863) + N't';
DECLARE @Vang NVARCHAR(50) = N'V' + NCHAR(7855) + N'ng';
DECLARE @CoPhep NVARCHAR(50) = N'C' + NCHAR(243) + N' ph' + NCHAR(233) + N'p';
DECLARE @DiMuon NVARCHAR(50) = NCHAR(272) + N'i mu' + NCHAR(7897) + N'n';

UPDATE [DiemDanhs] SET [TrangThai] = 'PRESENT' WHERE [TrangThai] IN (@CoMat, 'Co mat', 'CoMat', 'PRESENT', 'present');
UPDATE [DiemDanhs] SET [TrangThai] = 'ABSENT'  WHERE [TrangThai] IN (@Vang, 'Vang', 'ABSENT', 'absent');
UPDATE [DiemDanhs] SET [TrangThai] = 'EXCUSED' WHERE [TrangThai] IN (@CoPhep, 'Co phep', 'CoPhep', 'EXCUSED', 'excused');
UPDATE [DiemDanhs] SET [TrangThai] = 'LATE'    WHERE [TrangThai] IN (@DiMuon, 'Di muon', 'DiMuon', 'LATE', 'late');

-- 3. Xóa các bản ghi trùng lặp (giữ lại bản ghi có Id lớn nhất)
PRINT N'3. Dọn dẹp bản ghi trùng lặp (giữ Id lớn nhất)...';
WITH RankedAttendance AS (
    SELECT [Id],
           ROW_NUMBER() OVER (
               PARTITION BY [SinhVienId], [LopHocPhanId], [NgayDiemDanh]
               ORDER BY [Id] DESC
           ) AS rn
    FROM [DiemDanhs]
    WHERE [LopHocPhanId] IS NOT NULL
)
DELETE FROM [DiemDanhs]
WHERE [Id] IN (
    SELECT [Id] FROM RankedAttendance WHERE rn > 1
);

-- 4. Chuyển kiểu cột NgayDiemDanh sang DATE (nếu chưa chuyển)
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[DiemDanhs]') 
      AND name = N'NgayDiemDanh' 
      AND system_type_id <> TYPE_ID('date')
)
BEGIN
    PRINT N'4. Cập nhật kiểu dữ liệu cột NgayDiemDanh thành DATE...';
    DECLARE @defConstraint NVARCHAR(255);
    SELECT @defConstraint = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    WHERE d.parent_object_id = OBJECT_ID(N'[DiemDanhs]') AND c.name = N'NgayDiemDanh';

    IF @defConstraint IS NOT NULL
        EXEC('ALTER TABLE [DiemDanhs] DROP CONSTRAINT ' + @defConstraint);

    ALTER TABLE [DiemDanhs] ALTER COLUMN [NgayDiemDanh] DATE NOT NULL;
END;

-- 5. Tạo Unique Filtered Index chống trùng điểm danh trong cùng một ngày
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DiemDanhs_SinhVien_LopHocPhan_Ngay' AND object_id = OBJECT_ID(N'[DiemDanhs]'))
BEGIN
    PRINT N'5. Tạo Unique Filtered Index: IX_DiemDanhs_SinhVien_LopHocPhan_Ngay...';
    CREATE UNIQUE INDEX [IX_DiemDanhs_SinhVien_LopHocPhan_Ngay]
    ON [DiemDanhs] ([SinhVienId], [LopHocPhanId], [NgayDiemDanh])
    WHERE [LopHocPhanId] IS NOT NULL;
END;

-- 6. Tạo Index hỗ trợ truy vấn lọc theo buổi học
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DiemDanhs_LopHocPhan_Ngay' AND object_id = OBJECT_ID(N'[DiemDanhs]'))
BEGIN
    PRINT N'6. Tạo Index: IX_DiemDanhs_LopHocPhan_Ngay...';
    CREATE INDEX [IX_DiemDanhs_LopHocPhan_Ngay]
    ON [DiemDanhs] ([LopHocPhanId], [NgayDiemDanh]);
END;

-- 7. Ghi nhận lịch sử migration vào __EFMigrationsHistory
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261108000512_NormalizeAttendanceStatusAndAddConstraints')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261108000512_NormalizeAttendanceStatusAndAddConstraints', N'10.0.13');
        PRINT N'7. Đã ghi nhận migration Phase 1 vào __EFMigrationsHistory.';
    END;
END;

COMMIT TRANSACTION;
PRINT N'>> Hoàn tất thực thi script Phase 1 thành công!';
GO
