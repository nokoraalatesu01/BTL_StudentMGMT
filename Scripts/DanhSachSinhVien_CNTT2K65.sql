/*
Import danh sach sinh vien CNTT2-K65 tu PDF vao dbo.SinhViens.
Lop IT2 da xac nhan co Id = 2.
Khong gan NoiSinh vao DiaChi; GioiTinh/UserId/NienKhoa/TrangThai de NULL vi PDF khong cung cap.
Khong tu sua email/dia danh dang nghi van.
Cac MaSinhVien da ton tai se duoc bo qua de tranh chen trung.
*/

use BTL_StudentMGMTDb
go

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.Lops WHERE Id = 2 AND TenLop = N'IT2')
BEGIN
    RAISERROR(N'Khong tim thay lop IT2 co Id = 2. Hay kiem tra dbo.Lops truoc khi chay.', 16, 1);
RETURN;
END;

BEGIN TRANSACTION;

DECLARE @Nguon TABLE
(
    STT_PDF INT NOT NULL,
    MaSinhVien NVARCHAR(50) NOT NULL,
    HoTen NVARCHAR(200) NOT NULL,
    NgaySinh DATE NULL,
    Email NVARCHAR(320) NULL,
    SoDienThoai NVARCHAR(50) NULL,
    NoiSinhPDF NVARCHAR(100) NULL,
    CanKiemTra BIT NOT NULL
);

INSERT INTO @Nguon (STT_PDF, MaSinhVien, HoTen, NgaySinh, Email, SoDienThoai, NoiSinhPDF, CanKiemTra)
VALUES

(1, N'241230648', N'ĐỖ MINH AN', '2006-12-06', N'dominhan0612@gmail.com', N'333455447', N'Hà Nội', 0),
(2, N'241230651', N'NGUYỄN DUY AN', '2006-08-22', N'nguyenduyan082006@gmail.com', N'366255768', N'Hà Nội', 0),
(3, N'241224467', N'NGUYỄN VIỆT ANH', '2006-02-24', N'rtmx24@gmail.com', N'353536645', N'Hưng Yên', 0),
(4, N'241230658', N'PHẠM THỊ NGUYỆT ANH', '2006-02-21', N'phamanhdh.17@gmail.com', N'775005559', N'TP. Hồ Chí Minh', 0),
(5, N'241230659', N'THỊNH QUANG ANH', '2006-10-09', N'thinhquanganh0910206@gmail.com', N'774316784', N'Ninh Bình', 1),
(6, N'241224469', N'ĐẶNG TRẦN BIÊN', '2006-02-27', N'dangtranbien206@gmail.com', N'365336140', N'Hưng Yên', 0),
(7, N'241230664', N'NGUYỄN ĐỨC BẢO BÌNH', '2006-11-07', N'ndbb7112006@gmail.com', N'847112006', N'Hà Nội', 0),
(8, N'241230671', N'TRẦN XUÂN TUẤN CƯỜNG', '2006-02-12', N'tcuongabc672@gmail.com', N'904103447', N'Hưng Yên', 0),
(9, N'241230672', N'TRƯƠNG TUẤN CƯỜNG', '2006-01-10', N'cuong60126@gmail.com', N'977164361', N'Hà Nội', 0),
(10, N'241230665', N'NGUYỄN THÙY CHI', '2006-06-03', N'nguyenthuychi0306@gmail.com', N'337201062', N'Ninh Bình', 0),
(11, N'241230669', N'NGÔ MINH CHÍNH', '2006-01-12', N'nmchinh14@gmail.com', N'788248976', N'Bắc Ninh', 0),
(12, N'241230675', N'PHAN VĂN DIỄN', '2006-06-29', N'phandien29062006@gmail.com', N'347580856', N'Ninh Bình', 0),
(13, N'241230684', N'MAI ĐỨC DUY', '2006-10-15', N'maiducduy15102006@gmail.com', N'982386712', N'Ninh Bình', 0),
(14, N'241230686', N'NGUYỄN ĐỨC DUY', '2006-04-04', N'nguyenducduy04042006@gmail.com', N'865049565', N'Hà Nội', 0),
(15, N'241230691', N'NGUYỄN THÁI DƯƠNG', '2006-05-06', N'nguyenthaiduong2006.tohieu@gmail.com', N'847371786', N'Ninh Bình', 0),
(16, N'241230706', N'NGÔ HOÀNG ĐỈNH', '2006-12-14', N'ngohoangdinh06@gmail.com', N'971356723', N'Ninh Bình', 0),
(17, N'241230707', N'NGUYỄN VĂN ĐÔNG', '2006-11-29', N'vandong2006bg@gmail.com', N'986797564', N'Bắc Ninh', 0),
(18, N'241230709', N'LÊ MINH ĐỨC', '2006-02-07', N'lmd388222@gmail.com', N'335533023', N'Hà Nội', 0),
(19, N'241230716', N'NGUYỄN THANH HẢI', '2006-06-17', N'hainguyen17062006@gmail.com', N'355245157', N'Phú Thọ', 0),
(20, N'241230719', N'HOÀNG THU HẰNG', '2006-09-21', N'Hoanghang210926@gmail.com', N'348890210', N'Bắc Ninh', 0),
(21, N'241230725', N'NGUYỄN ANH HIẾU', '2006-05-03', N'cupitcookies@gmail.com', N'354031024', N'Hà Nội', 0),
(22, N'241230734', N'NGUYỄN HUY HOÀNG', '2006-04-04', N'Hoangsml2006@gmail.com', N'964461601', N'Hưng Yên', 0),
(23, N'241230736', N'NGUYỄN HỮU HOÀNH', '2006-01-25', N'nguyenhuuhoanh2227@gmail.com', N'362035376', N'Hà Tĩnh', 0),
(24, N'241230739', N'NGUYỄN HUY HÙNG', '2006-07-23', N'hunghuykeet@gmail.com', N'398870266', N'Hà Nội', 0),
(25, N'241230742', N'DƯƠNG QUANG HUY', '2006-11-23', N'hinsiki11@gmail.com', N'349887716', N'Hà Nội', 0),
(26, N'241224478', N'NGUYỄN ĐỨC HUY', '2006-10-11', N'ng.duchuy2k6@gmail.com', N'335778132', N'Quảng Ninh', 0),
(27, N'241230749', N'TRẦN QUANG HUY', '2006-10-22', N'ntquanghuy2210@gmail.com', N'815532569', N'Ninh Bình', 0),
(28, N'241230756', N'TRẦN QUANG KHẢI', '2006-11-19', N'tqk1911@gmail.com', N'948172441', N'Hà Nội', 0),
(29, N'241230762', N'HOÀNG NGỌC KHOA', '2005-10-22', N'khoaz2210@gmail.com', N'865865135', N'Thanh Hoá', 0),
(30, N'241230763', N'TRẦN ĐÌNH KHOA', '2006-06-23', N'Tdkhoa23062006@gmail.com', N'961845462', N'Hưng Yên', 0),
(31, N'241230775', N'LÊ THỊ DIỆU LINH', '2006-11-30', N'ledieulinh3011@gmail.com', N'775353069', N'Hải Phòng', 0),
(32, N'241230777', N'BÙI XUÂN LONG', '2006-08-01', N'xuanlong1806@gmail.com', N'347109478', N'Đồng Nai', 0),
(33, N'241230780', N'TRẦN HỮU LONG', '2006-12-23', N'culong2312@gmail.com', N'364478366', N'Phú Thọ', 0),
(34, N'241230781', N'NGUYỄN THỊ MINH LỘC', '2006-10-12', N'nguyenthiminhloc12102006@gmail.com', N'357749143', N'Hưng Yên', 0),
(35, N'241230784', N'NGUYỄN THỊ XUÂN MAI', '2006-03-30', N'nguyenmaii3003@gmail.com', N'378093269', N'Phú Thọ', 0),
(36, N'231230835', N'HOÀNG BÌNH MẠNH', '2005-10-16', N'manh6012@gmail.com', N'396703136', N'Thanh Hoá', 0),
(37, N'241230793', N'BÙI QUANG MINH', '2005-10-06', N'buiquangminh0610@gmail.com', N'965299505', N'Hà Nội', 0),
(38, N'241224482', N'NGUYỄN KHẮC MINH', '2006-09-08', N'minhnk46@gmail.com', N'559499662', N'Bắc Giang', 0),
(39, N'241230795', N'NGUYỄN TUẤN MINH', '2006-04-08', N'tmiikka.8406@gmail.com', N'979430849', N'Hà Nội', 0),
(40, N'241230796', N'PHẠM CÔNG MINH', '2006-07-14', N'Pcm14072006@gmail.com', N'333368774', N'Cà Mau', 0),
(41, N'241230799', N'ĐINH THÀNH NAM', '2006-12-27', N'boptom2006@gmail.com', N'981020688', N'Hà Nội', 0),
(42, N'241230800', N'HƯỚNG XUÂN NAM', '2006-03-07', N'boy1champdextra@gmail.com', N'373199006', N'Bắc Ninh', 0),
(43, N'241230802', N'TRẦN HẢI NAM', '2006-06-30', N'trzhainam3006@gmail.com', N'968198265', N'Hà Nội', 0),
(44, N'241230808', N'ĐINH TRƯỜNG NGUYÊN', '2006-09-10', N'dtruongnguyen085gmail.com', N'977596340', N'Hà Nội', 1),
(45, N'241224486', N'ĐÀM VĂN PHONG', '2006-05-06', N'Nhokadzzmin@gmail.com', N'961199627', N'Bắc Ninh', 0),
(46, N'241230817', N'TRẦN ĐÌNH PHÚ', '2006-03-08', N'trandinhphu569@gmail.com', N'773353422', N'Nghệ An', 0),
(47, N'241230824', N'CHU MINH QUÂN', '2006-04-30', N'mquan304206@gmail.com', N'977083264', N'Nghệ An', 0),
(48, N'241230827', N'LÊ MINH QUÂN', '2006-04-28', N'leminhquanhm@gmail.com', N'981807384', N'Nghê An', 1),
(48, N'241230859', N'ĐỖ VIỆT TIẾN', '2006-07-06', N'iamdoviettien@gmail.com', N'346440807', N'Hà Nội', 1),
(50, N'241230866', N'TRẦN ĐỨC TOÀN', '2006-10-08', N'toan12dpt@gmail.com', N'973202596', N'Hải Phòng', 0),
(51, N'241224493', N'ĐẶNG ANH TÚ', '2006-12-03', N'dagtu8888@gmail.com', N'343310068', N'Hà Nội', 0),
(52, N'241230879', N'NGUYỄN VĂN TÚ', '2006-09-05', N'nguyentu05092006@gmail.com', N'326466758', N'Hà Tây', 0),
(53, N'241230883', N'PHẠM ANH TUẤN', '2006-05-09', N'tnau06@gmail.com', N'334564031', N'Ninh Bình', 0),
(54, N'241230885', N'VŨ DUY TUẤN', '2006-04-25', N'vuduytuan25042006@gmail.com', N'904132677', N'Hưng Yên', 0),
(55, N'241230845', N'NGUYỄN HỮU THÀNH', '2006-12-26', N'thanhhngu06@gmail.com', N'986274130', N'Bắc Ninh', 0),
(56, N'241230846', N'NGUYỄN TRUNG THÀNH', '2006-10-15', N'trungthanh151006@gmail.com', N'329895506', N'Hà Tĩnh', 0),
(57, N'241230852', N'NGUYỄN TRUNG THẮNG', '2006-05-09', N'dnt2k07@gmail.com', N'869936206', N'Hà Tây', 0),
(58, N'241230854', N'NGUYỄN ĐỨC THIỆN', '2006-09-09', N'nguyenthientt06@gmail.com', N'868285892', N'Ninh Bình', 0),
(59, N'241224491', N'HOÀNG BÁ TRUNG', '2006-01-12', N'batrunghoang832@gmail.com', N'354120145', N'Bắc Ninh', 0),
(60, N'241230871', N'ĐINH KHẮC TRƯỜNG', '2006-04-23', N'dinhtruong2342006@gmail.com', N'866265325', N'Nam Định', 0),
(61, N'241230872', N'NGUYỄN ĐỨC TRƯỜNG', '2006-12-12', N'alatesu01@gmail.com', N'335363882', N'Ninh Bình', 0),
(62, N'241230876', N'LÊ QUỐC TRƯỞNG', '2006-06-27', N'quoctruongle766@gmail.com', N'367135874', N'Thái Bình', 0),
(63, N'241230889', N'ĐINH ĐỨC VĂN', '2006-08-01', N'ducvan182006@gmail.com', N'385286912', N'Hưng Yên', 0),
(64, N'241230893', N'TRẦN CÔNG VINH', '2006-07-12', N'Vinhtran12p@gmail.com', N'977491526', N'Hà Tĩnh', 0);


-- Luu truoc cac ma sinh vien da ton tai de bao cao dung cac dong bi bo qua.
DECLARE @DaTonTai TABLE (STT_PDF INT, MaSinhVien NVARCHAR(50), HoTen NVARCHAR(200));
INSERT INTO @DaTonTai (STT_PDF, MaSinhVien, HoTen)
SELECT n.STT_PDF, n.MaSinhVien, n.HoTen
FROM @Nguon AS n
WHERE EXISTS (SELECT 1 FROM dbo.SinhViens AS sv WHERE sv.MaSinhVien = n.MaSinhVien);

-- Chi chen cac sinh vien chua ton tai theo MaSinhVien.
INSERT INTO dbo.SinhViens
    (MaSinhVien, HoTen, NgaySinh, Email, SoDienThoai, LopId, GioiTinh, DiaChi, UserId, NienKhoa, TrangThai)
SELECT
    n.MaSinhVien,
    n.HoTen,
    n.NgaySinh,
    n.Email,
    n.SoDienThoai,
    2,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL
FROM @Nguon AS n
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.SinhViens AS sv
    WHERE sv.MaSinhVien = n.MaSinhVien
);

DECLARE @SoDaChen INT = @@ROWCOUNT;

-- Bao cao cac dong duoc bo qua do MaSinhVien da ton tai.
SELECT STT_PDF, MaSinhVien, HoTen, N'Da ton tai trong dbo.SinhViens - bo qua' AS KetQua
FROM @DaTonTai
ORDER BY STT_PDF;

-- Danh sach can doi chieu theo PDF: email nghi van, STT lap, noi sinh ghi bat thuong.
SELECT STT_PDF, MaSinhVien, HoTen, Email, NoiSinhPDF,
       CASE WHEN CanKiemTra = 1 THEN N'Can doi chieu voi PDF goc' ELSE N'' END AS GhiChuKiemTra
FROM @Nguon
WHERE CanKiemTra = 1
ORDER BY STT_PDF;

COMMIT TRANSACTION;
PRINT N'So sinh vien moi da chen: ' + CONVERT(NVARCHAR(20), @SoDaChen);

-- Xem danh sach sinh vien lop IT2 sau khi nap.
SELECT Id, MaSinhVien, HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, DiaChi, LopId, UserId, NienKhoa, TrangThai
FROM dbo.SinhViens
WHERE LopId = 2
ORDER BY MaSinhVien;
