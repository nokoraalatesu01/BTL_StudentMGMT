-- =========================================================================
-- SCRIPT CHẠY TAY THỦ CÔNG: PHASE 3 - BỔ SUNG CÁC TRƯỜNG AUDIT CHO ĐIỂM DANH
-- Khuyến nghị: Thực hiện SAO LƯU (BACKUP) cơ sở dữ liệu trước khi chạy script này!
-- Script này được thiết kế IDEMPOTENT (có thể chạy an toàn nhiều lần).
-- Phù hợp cho database đã bị lệch hoặc không chạy được qua dotnet ef database update.
-- =========================================================================

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'>> Bắt đầu thực thi script bổ sung trường Audit Phase 3...';

-- 1. Thêm cột NguoiDiemDanh (nvarchar(100), nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[DiemDanhs]') AND name = N'NguoiDiemDanh'
)
BEGIN
    PRINT N'1. Thêm cột [NguoiDiemDanh] vào bảng [DiemDanhs]...';
    ALTER TABLE [DiemDanhs] ADD [NguoiDiemDanh] NVARCHAR(100) NULL;
END
ELSE
BEGIN
    PRINT N'1. Cột [NguoiDiemDanh] đã tồn tại, bỏ qua.';
END;

-- 2. Thêm cột NgayTao (datetime2, nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[DiemDanhs]') AND name = N'NgayTao'
)
BEGIN
    PRINT N'2. Thêm cột [NgayTao] vào bảng [DiemDanhs]...';
    ALTER TABLE [DiemDanhs] ADD [NgayTao] DATETIME2 NULL;
END
ELSE
BEGIN
    PRINT N'2. Cột [NgayTao] đã tồn tại, bỏ qua.';
END;

-- 3. Thêm cột NguoiSua (nvarchar(100), nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[DiemDanhs]') AND name = N'NguoiSua'
)
BEGIN
    PRINT N'3. Thêm cột [NguoiSua] vào bảng [DiemDanhs]...';
    ALTER TABLE [DiemDanhs] ADD [NguoiSua] NVARCHAR(100) NULL;
END
ELSE
BEGIN
    PRINT N'3. Cột [NguoiSua] đã tồn tại, bỏ qua.';
END;

-- 4. Thêm cột NgaySua (datetime2, nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[DiemDanhs]') AND name = N'NgaySua'
)
BEGIN
    PRINT N'4. Thêm cột [NgaySua] vào bảng [DiemDanhs]...';
    ALTER TABLE [DiemDanhs] ADD [NgaySua] DATETIME2 NULL;
END
ELSE
BEGIN
    PRINT N'4. Cột [NgaySua] đã tồn tại, bỏ qua.';
END;

-- 5. Ghi nhận lịch sử migration vào __EFMigrationsHistory
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261108010449_AddAttendanceAuditFields')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261108010449_AddAttendanceAuditFields', N'10.0.13');
        PRINT N'5. Đã ghi nhận migration AddAttendanceAuditFields vào __EFMigrationsHistory.';
    END
    ELSE
    BEGIN
        PRINT N'5. Bản ghi migration đã tồn tại trong __EFMigrationsHistory, bỏ qua.';
    END;
END;

COMMIT TRANSACTION;
PRINT N'>> Hoàn tất thực thi script Phase 3 thành công!';
GO
