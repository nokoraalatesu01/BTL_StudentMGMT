-- ==================================================================================
-- HỆ THỐNG QUẢN LÝ SINH VIÊN - BTL_StudentMGMT
-- SCRIPT KIỂM TRA TOÀN DIỆN DATABASE SCHEMA VÀ TOÀN VẸN DỮ LIỆU
-- ==================================================================================
-- Script này được thiết kế chỉ để KIỂM TRA (READ-ONLY)
-- KHÔNG tự động xóa, cập nhật hoặc sửa đổi schema/dữ liệu.
-- ==================================================================================

USE [BTL_StudentMGMTDb];
GO

PRINT '==================================================================================';
PRINT '1. DANH SÁCH BẢNG HIỆN TẠI TRONG CƠ SỞ DỮ LIỆU';
PRINT '==================================================================================';
SELECT 
    t.TABLE_SCHEMA AS [Schema],
    t.TABLE_NAME AS [TableName],
    p.rows AS [RowCount]
FROM INFORMATION_SCHEMA.TABLES t
INNER JOIN sys.tables st ON st.name = t.TABLE_NAME
INNER JOIN sys.partitions p ON p.object_id = st.object_id AND p.index_id IN (0, 1)
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME;
GO

PRINT '==================================================================================';
PRINT '2. LỊCH SỬ MIGRATION ĐÃ ÁP DỤNG TRONG __EFMigrationsHistory';
PRINT '==================================================================================';
IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NOT NULL
BEGIN
    SELECT 
        MigrationId, 
        ProductVersion 
    FROM dbo.__EFMigrationsHistory 
    ORDER BY MigrationId;
END
ELSE
BEGIN
    PRINT 'CẢNH BÁO: Không tìm thấy bảng __EFMigrationsHistory!';
END;
GO

PRINT '==================================================================================';
PRINT '3. DANH SÁCH KHÓA CHÍNH (PRIMARY KEYS)';
PRINT '==================================================================================';
SELECT 
    kcu.TABLE_NAME AS [TableName],
    kcu.COLUMN_NAME AS [ColumnName],
    tc.CONSTRAINT_NAME AS [ConstraintName]
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME 
    AND tc.TABLE_SCHEMA = kcu.TABLE_SCHEMA
WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
ORDER BY kcu.TABLE_NAME;
GO

PRINT '==================================================================================';
PRINT '4. DANH SÁCH KHÓA NGOẠI (FOREIGN KEYS) VÀ QUY TẮC XÓA (DELETE_RULE)';
PRINT '==================================================================================';
SELECT 
    fk.name AS [FK_Name],
    OBJECT_NAME(fk.parent_object_id) AS [ChildTable],
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS [ChildColumn],
    OBJECT_NAME(fk.referenced_object_id) AS [ParentTable],
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS [ParentColumn],
    fk.delete_referential_action_desc AS [OnDeleteAction],
    fk.update_referential_action_desc AS [OnUpdateAction]
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc 
    ON fk.object_id = fc.constraint_object_id
ORDER BY [ChildTable], [FK_Name];
GO

PRINT '==================================================================================';
PRINT '5. DANH SÁCH INDEX VÀ RÀNG BUỘC UNIQUE';
PRINT '==================================================================================';
SELECT 
    OBJECT_NAME(i.object_id) AS [TableName],
    i.name AS [IndexName],
    i.type_desc AS [IndexType],
    i.is_unique AS [IsUnique],
    i.is_primary_key AS [IsPrimaryKey],
    i.has_filter AS [HasFilter],
    i.filter_definition AS [FilterDefinition]
FROM sys.indexes i
INNER JOIN sys.tables t ON t.object_id = i.object_id
WHERE i.name IS NOT NULL
ORDER BY [TableName], [IndexName];
GO

PRINT '==================================================================================';
PRINT '6. KIỂM TRA DỮ LIỆU MỒ CÔI (ORPHANED RECORDS) GIỮA CÁC BẢNG';
PRINT '==================================================================================';

-- 6.1 Sinh viên tham chiếu Lớp không tồn tại
IF OBJECT_ID('dbo.SinhViens', 'U') IS NOT NULL AND OBJECT_ID('dbo.Lops', 'U') IS NOT NULL
BEGIN
    SELECT sv.Id, sv.MaSinhVien, sv.HoTen, sv.LopId
    FROM dbo.SinhViens sv
    LEFT JOIN dbo.Lops l ON sv.LopId = l.Id
    WHERE sv.LopId IS NOT NULL AND l.Id IS NULL;
    IF @@ROWCOUNT = 0 PRINT 'OK: Không có sinh viên mồ côi LopId.';
END;

-- 6.2 Lớp tham chiếu Ngành không tồn tại
IF OBJECT_ID('dbo.Lops', 'U') IS NOT NULL AND OBJECT_ID('dbo.Nganhs', 'U') IS NOT NULL
BEGIN
    SELECT l.Id, l.TenLop, l.NganhId
    FROM dbo.Lops l
    LEFT JOIN dbo.Nganhs n ON l.NganhId = n.Id
    WHERE n.Id IS NULL;
    IF @@ROWCOUNT = 0 PRINT 'OK: Không có lớp mồ côi NganhId.';
END;

-- 6.3 Điểm tham chiếu Sinh viên hoặc Môn học không tồn tại
IF OBJECT_ID('dbo.Diems', 'U') IS NOT NULL
BEGIN
    SELECT d.Id, d.SinhVienId, d.MonHocId
    FROM dbo.Diems d
    LEFT JOIN dbo.SinhViens sv ON d.SinhVienId = sv.Id
    LEFT JOIN dbo.MonHocs mh ON d.MonHocId = mh.Id
    WHERE sv.Id IS NULL OR mh.Id IS NULL;
    IF @@ROWCOUNT = 0 PRINT 'OK: Không có điểm mồ côi SinhVienId/MonHocId.';
END;

-- 6.4 Giảng viên tham chiếu Đơn vị không tồn tại
IF OBJECT_ID('dbo.GiangViens', 'U') IS NOT NULL AND OBJECT_ID('dbo.DonVi', 'U') IS NOT NULL
BEGIN
    SELECT gv.Id, gv.MaGiangVien, gv.HoTen, gv.DonViId
    FROM dbo.GiangViens gv
    LEFT JOIN dbo.DonVi dv ON gv.DonViId = dv.DonViId
    WHERE gv.DonViId IS NOT NULL AND dv.DonViId IS NULL;
    IF @@ROWCOUNT = 0 PRINT 'OK: Không có giảng viên mồ côi DonViId.';
END;

-- 6.5 Môn học tham chiếu Đơn vị không tồn tại
IF OBJECT_ID('dbo.MonHocs', 'U') IS NOT NULL AND OBJECT_ID('dbo.DonVi', 'U') IS NOT NULL
BEGIN
    SELECT mh.Id, mh.MaMon, mh.TenMon, mh.DonViId
    FROM dbo.MonHocs mh
    LEFT JOIN dbo.DonVi dv ON mh.DonViId = dv.DonViId
    WHERE mh.DonViId IS NOT NULL AND dv.DonViId IS NULL;
    IF @@ROWCOUNT = 0 PRINT 'OK: Không có môn học mồ côi DonViId.';
END;

-- 6.6 Ngành tham chiếu Đơn vị không tồn tại
IF OBJECT_ID('dbo.Nganhs', 'U') IS NOT NULL AND OBJECT_ID('dbo.DonVi', 'U') IS NOT NULL
BEGIN
    SELECT n.Id, n.MaNganh, n.TenNganh, n.DonViId
    FROM dbo.Nganhs n
    LEFT JOIN dbo.DonVi dv ON n.DonViId = dv.DonViId
    WHERE n.DonViId IS NOT NULL AND dv.DonViId IS NULL;
    IF @@ROWCOUNT = 0 PRINT 'OK: Không có ngành mồ côi DonViId.';
END;

-- 6.7 Đơn vị tham chiếu Đơn vị cha không tồn tại (vòng tròn hoặc mồ côi)
IF OBJECT_ID('dbo.DonVi', 'U') IS NOT NULL
BEGIN
    SELECT dv.DonViId, dv.MaDonVi, dv.TenDonVi, dv.DonViChaId
    FROM dbo.DonVi dv
    LEFT JOIN dbo.DonVi cha ON dv.DonViChaId = cha.DonViId
    WHERE dv.DonViChaId IS NOT NULL AND cha.DonViId IS NULL;
    IF @@ROWCOUNT = 0 PRINT 'OK: Không có đơn vị mồ côi DonViChaId.';
END;
GO

PRINT '==================================================================================';
PRINT '7. THỐNG KÊ CÁC BẢNG VÀ CỘT KHÓA NGOẠI CÓ DỮ LIỆU NULL';
PRINT '==================================================================================';
IF OBJECT_ID('dbo.GiangViens', 'U') IS NOT NULL
    SELECT COUNT(*) AS [GiangVien_Total], SUM(CASE WHEN DonViId IS NULL THEN 1 ELSE 0 END) AS [GiangVien_DonViNull] FROM dbo.GiangViens;

IF OBJECT_ID('dbo.MonHocs', 'U') IS NOT NULL
    SELECT COUNT(*) AS [MonHoc_Total], SUM(CASE WHEN DonViId IS NULL THEN 1 ELSE 0 END) AS [MonHoc_DonViNull] FROM dbo.MonHocs;

IF OBJECT_ID('dbo.Nganhs', 'U') IS NOT NULL
    SELECT COUNT(*) AS [Nganh_Total], SUM(CASE WHEN DonViId IS NULL THEN 1 ELSE 0 END) AS [Nganh_DonViNull] FROM dbo.Nganhs;

IF OBJECT_ID('dbo.Lops', 'U') IS NOT NULL
    SELECT COUNT(*) AS [Lop_Total], SUM(CASE WHEN ChuongTrinhDaoTaoId IS NULL THEN 1 ELSE 0 END) AS [Lop_ChuongTrinhNull] FROM dbo.Lops;

PRINT '==================================================================================';
PRINT 'HOÀN THÀNH KIỂM TRA SCHEMA VÀ DỮ LIỆU DATABASE.';
PRINT '==================================================================================';
