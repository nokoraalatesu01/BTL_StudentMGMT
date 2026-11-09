-- =========================================================================
-- SCRIPT: Audit_LopHocPhan_CaViolations.sql
-- MỤC ĐÍCH: Rà soát các lớp học phần (LopHocPhan) vi phạm quy tắc nghiệp vụ mới:
--           "Mỗi lớp học phần chỉ được đúng 1 khung giờ = 1 thứ + 1 ca (Ca 1..5)
--           với SoTiet = 2 và TietBatDau thuộc {1, 3, 5, 7, 9}".
-- LƯU Ý: Không tự động sửa hoặc xóa dữ liệu. Script này dùng để liệt kê và
--        đề xuất phương án chuẩn hóa khi được phê duyệt.
-- =========================================================================

USE [BTL_StudentMGMTDb];
GO

-- 1. LIỆT KÊ TẤT CẢ CÁC LỚP HỌC PHẦN VI PHẠM QUY TẮC 1 CA HỌC
PRINT N'=== DANH SÁCH LỚP HỌC PHẦN VI PHẠM QUY TẮC 1 CA (SoTiet <> 2 HOẶC TietBatDau KHÔNG THUỘC {1,3,5,7,9}) ===';

SELECT 
    Id,
    MaLopHocPhan,
    TenLopHocPhan,
    Thu,
    TietBatDau,
    SoTiet,
    (TietBatDau + SoTiet - 1) AS TietKetThuc,
    PhongHoc,
    TrangThai,
    CASE 
        WHEN SoTiet <> 2 AND TietBatDau NOT IN (1, 3, 5, 7, 9) 
            THEN N'Sai cả tiết bắt đầu (' + CAST(TietBatDau AS NVARCHAR) + N') và số tiết (' + CAST(SoTiet AS NVARCHAR) + N')'
        WHEN SoTiet <> 2 
            THEN N'Số tiết (' + CAST(SoTiet AS NVARCHAR) + N') khác 2 (vắt ca hoặc độ dài không đúng chuẩn)'
        WHEN TietBatDau NOT IN (1, 3, 5, 7, 9) 
            THEN N'Tiết bắt đầu (' + CAST(TietBatDau AS NVARCHAR) + N') không phải đầu ca {1, 3, 5, 7, 9}'
        ELSE N'Hợp lệ'
    END AS LyDoViPham,
    -- Gợi ý ca tương ứng theo tiết bắt đầu
    CASE 
        WHEN TietBatDau = 1 THEN N'Ca 1 (07:00 – 09:30, Tiết 1–2)'
        WHEN TietBatDau = 3 THEN N'Ca 2 (09:35 – 12:00, Tiết 3–4)'
        WHEN TietBatDau = 5 THEN N'Ca 3 (13:00 – 15:30, Tiết 5–6)'
        WHEN TietBatDau = 7 THEN N'Ca 4 (15:35 – 18:00, Tiết 7–8)'
        WHEN TietBatDau = 9 THEN N'Ca 5 (18:15 – 20:45, Tiết 9–10)'
        ELSE N'Cần người quản trị chọn ca phù hợp'
    END AS GoiYCaHoc
FROM LopHocPhan
WHERE TietBatDau NOT IN (1, 3, 5, 7, 9) OR SoTiet <> 2
ORDER BY Id;

-- 2. THỐNG KÊ TỔNG QUAN
SELECT 
    COUNT(*) AS TongSoLopViPham,
    SUM(CASE WHEN SoTiet <> 2 THEN 1 ELSE 0 END) AS SoLopSaiSoTiet,
    SUM(CASE WHEN TietBatDau NOT IN (1, 3, 5, 7, 9) THEN 1 ELSE 0 END) AS SoLopSaiTietDauCa
FROM LopHocPhan
WHERE TietBatDau NOT IN (1, 3, 5, 7, 9) OR SoTiet <> 2;
GO

/*
================================================================================
KẾT QUẢ RÀ SOÁT THỰC TẾ TRONG DATABASE (Đã ghi nhận 5 bản ghi vi phạm):
1. Id = 1 | INT301.01 | Thứ 4 | TietBatDau = 1 | SoTiet = 3 | TietKetThuc = 3 -> Vi phạm SoTiet = 3
2. Id = 2 | INT201.01 | Thứ 2 | TietBatDau = 1 | SoTiet = 4 | TietKetThuc = 4 -> Vi phạm SoTiet = 4 (vắt sang Ca 2)
3. Id = 3 | INT205.01 | Thứ 3 | TietBatDau = 7 | SoTiet = 3 | TietKetThuc = 9 -> Vi phạm SoTiet = 3 (vắt sang Ca 5)
4. Id = 4 | BUS101.01 | Thứ 3 | TietBatDau = 1 | SoTiet = 3 | TietKetThuc = 3 -> Vi phạm SoTiet = 3
5. Id = 5 | ENG201.01 | Thứ 6 | TietBatDau = 7 | SoTiet = 3 | TietKetThuc = 9 -> Vi phạm SoTiet = 3 (vắt sang Ca 5)

================================================================================
PHƯƠNG ÁN ĐỀ XUẤT ĐỂ NGƯỜI QUẢN TRỊ XEM XÉT VÀ QUYẾT ĐỊNH:

Phương án A (Khuyến nghị): Chuẩn hóa SoTiet = 2 cho cả 5 lớp theo ca tương ứng
-----------------------------------------------------------------------------
BEGIN TRANSACTION;

-- Lớp INT301.01: Thứ 4, Ca 1 (Tiết 1-2)
UPDATE LopHocPhan SET SoTiet = 2 WHERE Id = 1;

-- Lớp INT201.01: Thứ 2, Ca 1 (Tiết 1-2)
UPDATE LopHocPhan SET SoTiet = 2 WHERE Id = 2;

-- Lớp INT205.01: Thứ 3, Ca 4 (Tiết 7-8)
UPDATE LopHocPhan SET SoTiet = 2 WHERE Id = 3;

-- Lớp BUS101.01: Thứ 3, Ca 1 (Tiết 1-2)
UPDATE LopHocPhan SET SoTiet = 2 WHERE Id = 4;

-- Lớp ENG201.01: Thứ 6, Ca 4 (Tiết 7-8)
UPDATE LopHocPhan SET SoTiet = 2 WHERE Id = 5;

-- Kiểm tra lại sau cập nhật
-- SELECT * FROM LopHocPhan WHERE TietBatDau NOT IN (1, 3, 5, 7, 9) OR SoTiet <> 2;

-- COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;

Phương án B: Áp dụng CHECK CONSTRAINT với WITH NOCHECK
-----------------------------------------------------------------------------
-- Nếu giữ nguyên dữ liệu cũ mà vẫn muốn chặn các lớp mới vi phạm:
-- ALTER TABLE LopHocPhan WITH NOCHECK
-- ADD CONSTRAINT CK_LopHocPhan_SingleCa CHECK ([TietBatDau] IN (1, 3, 5, 7, 9) AND [SoTiet] = 2);
================================================================================
*/
