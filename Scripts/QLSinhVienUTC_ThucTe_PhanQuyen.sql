/*
===============================================================================
QLSinhVienUTC - CSDL quản lý sinh viên và phân quyền 4 vai trò
Target: SQL Server 2019+ (T-SQL)
Encoding: UTF-8
Lưu ý:
- Script tạo database mới nếu chưa tồn tại, không xóa dữ liệu đang có.
- Chạy bằng tài khoản có quyền CREATE DATABASE.
- Không lưu mật khẩu dạng rõ. ASP.NET Core Identity nên quản lý xác thực.
- Có 4 vai trò: ADMIN, PGV, GV, HS. Không tạo tài khoản/mật khẩu mặc định.
- Đây là schema nền tảng; các quy chế chính thức phải được trường xác nhận.
===============================================================================
*/
USE master;
GO
IF DB_ID(N'QLSinhVienUTC') IS NULL
    CREATE DATABASE QLSinhVienUTC;
GO
ALTER DATABASE QLSinhVienUTC SET RECOVERY SIMPLE;
ALTER DATABASE QLSinhVienUTC SET AUTO_CLOSE OFF;
ALTER DATABASE QLSinhVienUTC SET AUTO_SHRINK OFF;
GO
USE QLSinhVienUTC;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =============================== DANH MỤC =============================== */
IF OBJECT_ID('dbo.DonVi','U') IS NULL
CREATE TABLE dbo.DonVi (
    DonViId int IDENTITY(1,1) CONSTRAINT PK_DonVi PRIMARY KEY,
    MaDonVi varchar(20) NOT NULL,
    TenDonVi nvarchar(200) NOT NULL,
    LoaiDonVi varchar(20) NOT NULL, -- TRUONG, KHOA, BO_MON, PHONG
    DonViChaId int NULL,
    Email varchar(254) NULL,
    DienThoai varchar(20) NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_DonVi_TrangThai DEFAULT 'ACTIVE',
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_DonVi_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt datetime2(0) NULL,
    RowVer rowversion,
    CONSTRAINT UQ_DonVi_Ma UNIQUE(MaDonVi),
    CONSTRAINT CK_DonVi_Loai CHECK (LoaiDonVi IN ('TRUONG','KHOA','BO_MON','PHONG')),
    CONSTRAINT CK_DonVi_TrangThai CHECK (TrangThai IN ('ACTIVE','INACTIVE')),
    CONSTRAINT FK_DonVi_Cha FOREIGN KEY(DonViChaId) REFERENCES dbo.DonVi(DonViId)
);
GO

IF OBJECT_ID('dbo.NamHoc','U') IS NULL
CREATE TABLE dbo.NamHoc (
    NamHocId int IDENTITY(1,1) CONSTRAINT PK_NamHoc PRIMARY KEY,
    MaNamHoc varchar(9) NOT NULL, -- ví dụ 2026-2027
    NgayBatDau date NOT NULL,
    NgayKetThuc date NOT NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_NamHoc_TrangThai DEFAULT 'PLANNED',
    CONSTRAINT UQ_NamHoc_Ma UNIQUE(MaNamHoc),
    CONSTRAINT CK_NamHoc_Ngay CHECK(NgayBatDau < NgayKetThuc),
    CONSTRAINT CK_NamHoc_TrangThai CHECK(TrangThai IN ('PLANNED','ACTIVE','CLOSED'))
);
GO

IF OBJECT_ID('dbo.HocKy','U') IS NULL
CREATE TABLE dbo.HocKy (
    HocKyId int IDENTITY(1,1) CONSTRAINT PK_HocKy PRIMARY KEY,
    NamHocId int NOT NULL,
    MaHocKy varchar(20) NOT NULL, -- HK1, HK2, HE
    TenHocKy nvarchar(100) NOT NULL,
    NgayBatDau date NOT NULL,
    NgayKetThuc date NOT NULL,
    HanDangKyTu date NULL,
    HanDangKyDen date NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_HocKy_TrangThai DEFAULT 'PLANNED',
    CONSTRAINT FK_HocKy_NamHoc FOREIGN KEY(NamHocId) REFERENCES dbo.NamHoc(NamHocId),
    CONSTRAINT UQ_HocKy UNIQUE(NamHocId, MaHocKy),
    CONSTRAINT CK_HocKy_Ngay CHECK(NgayBatDau < NgayKetThuc),
    CONSTRAINT CK_HocKy_Han CHECK(HanDangKyTu IS NULL OR HanDangKyDen IS NULL OR HanDangKyTu <= HanDangKyDen),
    CONSTRAINT CK_HocKy_TrangThai CHECK(TrangThai IN ('PLANNED','REGISTRATION','IN_PROGRESS','GRADING','CLOSED'))
);
GO

IF OBJECT_ID('dbo.Nganh','U') IS NULL
CREATE TABLE dbo.Nganh (
    NganhId int IDENTITY(1,1) CONSTRAINT PK_Nganh PRIMARY KEY,
    MaNganh varchar(20) NOT NULL,
    TenNganh nvarchar(200) NOT NULL,
    DonViId int NOT NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_Nganh_TrangThai DEFAULT 'ACTIVE',
    CONSTRAINT UQ_Nganh_Ma UNIQUE(MaNganh),
    CONSTRAINT FK_Nganh_DonVi FOREIGN KEY(DonViId) REFERENCES dbo.DonVi(DonViId),
    CONSTRAINT CK_Nganh_TrangThai CHECK(TrangThai IN ('ACTIVE','INACTIVE'))
);
GO

IF OBJECT_ID('dbo.ChuongTrinhDaoTao','U') IS NULL
CREATE TABLE dbo.ChuongTrinhDaoTao (
    ChuongTrinhId int IDENTITY(1,1) CONSTRAINT PK_ChuongTrinhDaoTao PRIMARY KEY,
    MaChuongTrinh varchar(30) NOT NULL,
    TenChuongTrinh nvarchar(200) NOT NULL,
    NganhId int NOT NULL,
    TrinhDo varchar(30) NOT NULL CONSTRAINT DF_CTD_TrinhDo DEFAULT 'DAI_HOC',
    TongTinChiYeuCau smallint NOT NULL,
    SoNamDaoTao decimal(3,1) NULL,
    HieuLucTu date NOT NULL,
    HieuLucDen date NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_CTD_TrangThai DEFAULT 'ACTIVE',
    CONSTRAINT UQ_CTD_Ma UNIQUE(MaChuongTrinh),
    CONSTRAINT FK_CTD_Nganh FOREIGN KEY(NganhId) REFERENCES dbo.Nganh(NganhId),
    CONSTRAINT CK_CTD_TinChi CHECK(TongTinChiYeuCau > 0),
    CONSTRAINT CK_CTD_Nam CHECK(SoNamDaoTao IS NULL OR SoNamDaoTao > 0),
    CONSTRAINT CK_CTD_HieuLuc CHECK(HieuLucDen IS NULL OR HieuLucTu <= HieuLucDen),
    CONSTRAINT CK_CTD_TrangThai CHECK(TrangThai IN ('ACTIVE','INACTIVE'))
);
GO

IF OBJECT_ID('dbo.KhoaTuyenSinh','U') IS NULL
CREATE TABLE dbo.KhoaTuyenSinh (
    KhoaTuyenSinhId int IDENTITY(1,1) CONSTRAINT PK_KhoaTuyenSinh PRIMARY KEY,
    MaKhoa varchar(20) NOT NULL,
    TenKhoa nvarchar(100) NOT NULL,
    NamTuyenSinh smallint NOT NULL,
    CONSTRAINT UQ_KhoaTuyenSinh_Ma UNIQUE(MaKhoa),
    CONSTRAINT CK_KhoaTuyenSinh_Nam CHECK(NamTuyenSinh BETWEEN 1950 AND 2200)
);
GO

IF OBJECT_ID('dbo.LopHanhChinh','U') IS NULL
CREATE TABLE dbo.LopHanhChinh (
    LopId int IDENTITY(1,1) CONSTRAINT PK_LopHanhChinh PRIMARY KEY,
    MaLop varchar(30) NOT NULL,
    TenLop nvarchar(150) NOT NULL,
    NganhId int NOT NULL,
    ChuongTrinhId int NOT NULL,
    KhoaTuyenSinhId int NOT NULL,
    CoVanGiangVienId int NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_Lop_TrangThai DEFAULT 'ACTIVE',
    CONSTRAINT UQ_Lop_Ma UNIQUE(MaLop),
    CONSTRAINT FK_Lop_Nganh FOREIGN KEY(NganhId) REFERENCES dbo.Nganh(NganhId),
    CONSTRAINT FK_Lop_CTD FOREIGN KEY(ChuongTrinhId) REFERENCES dbo.ChuongTrinhDaoTao(ChuongTrinhId),
    CONSTRAINT FK_Lop_Khoa FOREIGN KEY(KhoaTuyenSinhId) REFERENCES dbo.KhoaTuyenSinh(KhoaTuyenSinhId),
    CONSTRAINT CK_Lop_TrangThai CHECK(TrangThai IN ('ACTIVE','GRADUATED','CLOSED'))
);
GO

/* =========================== NGƯỜI HỌC / NHÂN SỰ ========================= */
IF OBJECT_ID('dbo.SinhVien','U') IS NULL
CREATE TABLE dbo.SinhVien (
    SinhVienId int IDENTITY(1,1) CONSTRAINT PK_SinhVien PRIMARY KEY,
    MaSinhVien varchar(20) NOT NULL,
    Ho nvarchar(80) NOT NULL,
    TenDem nvarchar(100) NULL,
    Ten nvarchar(50) NOT NULL,
    NgaySinh date NULL,
    GioiTinh varchar(20) NULL,
    EmailTruong varchar(254) NULL,
    EmailCaNhan varchar(254) NULL,
    SoDienThoai varchar(20) NULL,
    CCCDHash varbinary(64) NULL, -- khuyến nghị không lưu CCCD nếu không cần; nếu cần thì mã hóa riêng
    DiaChiThuongTru nvarchar(500) NULL,
    DiaChiLienHe nvarchar(500) NULL,
    LopId int NOT NULL,
    NgayNhapHoc date NOT NULL,
    TrangThai varchar(25) NOT NULL CONSTRAINT DF_SinhVien_TrangThai DEFAULT 'STUDYING',
    IdentityUserId nvarchar(450) NULL, -- liên kết AspNetUsers.Id nếu dùng ASP.NET Identity
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_SinhVien_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt datetime2(0) NULL,
    RowVer rowversion,
    CONSTRAINT UQ_SinhVien_Ma UNIQUE(MaSinhVien),
    CONSTRAINT UQ_SinhVien_IdentityUser UNIQUE(IdentityUserId),
    CONSTRAINT FK_SinhVien_Lop FOREIGN KEY(LopId) REFERENCES dbo.LopHanhChinh(LopId),
    CONSTRAINT CK_SinhVien_GioiTinh CHECK(GioiTinh IS NULL OR GioiTinh IN ('NAM','NU','KHAC','KHONG_TIET_LO')),
    CONSTRAINT CK_SinhVien_TrangThai CHECK(TrangThai IN ('STUDYING','RESERVED','SUSPENDED','WITHDRAWN','GRADUATED'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SinhVien_Lop_TrangThai' AND object_id=OBJECT_ID('dbo.SinhVien'))
    CREATE INDEX IX_SinhVien_Lop_TrangThai ON dbo.SinhVien(LopId, TrangThai);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SinhVien_HoTen' AND object_id=OBJECT_ID('dbo.SinhVien'))
    CREATE INDEX IX_SinhVien_HoTen ON dbo.SinhVien(Ho, Ten);
GO

IF OBJECT_ID('dbo.GiangVien','U') IS NULL
CREATE TABLE dbo.GiangVien (
    GiangVienId int IDENTITY(1,1) CONSTRAINT PK_GiangVien PRIMARY KEY,
    MaGiangVien varchar(20) NOT NULL,
    Ho nvarchar(80) NOT NULL,
    TenDem nvarchar(100) NULL,
    Ten nvarchar(50) NOT NULL,
    Email varchar(254) NULL,
    SoDienThoai varchar(20) NULL,
    DonViId int NOT NULL,
    IdentityUserId nvarchar(450) NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_GiangVien_TrangThai DEFAULT 'ACTIVE',
    CONSTRAINT UQ_GiangVien_Ma UNIQUE(MaGiangVien),
    CONSTRAINT UQ_GiangVien_IdentityUser UNIQUE(IdentityUserId),
    CONSTRAINT FK_GiangVien_DonVi FOREIGN KEY(DonViId) REFERENCES dbo.DonVi(DonViId),
    CONSTRAINT CK_GiangVien_TrangThai CHECK(TrangThai IN ('ACTIVE','INACTIVE','RETIRED'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name='FK_Lop_CoVan' AND parent_object_id=OBJECT_ID('dbo.LopHanhChinh'))
    ALTER TABLE dbo.LopHanhChinh ADD CONSTRAINT FK_Lop_CoVan FOREIGN KEY(CoVanGiangVienId) REFERENCES dbo.GiangVien(GiangVienId);
GO

IF OBJECT_ID('dbo.LichSuTrangThaiSinhVien','U') IS NULL
CREATE TABLE dbo.LichSuTrangThaiSinhVien (
    LichSuId bigint IDENTITY(1,1) CONSTRAINT PK_LichSuTrangThaiSinhVien PRIMARY KEY,
    SinhVienId int NOT NULL,
    TrangThaiCu varchar(25) NULL,
    TrangThaiMoi varchar(25) NOT NULL,
    NgayHieuLuc date NOT NULL,
    LyDo nvarchar(500) NULL,
    NguoiThayDoi nvarchar(450) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_LichSuSV_Created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LichSuSV_SV FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId)
);
GO

/* =============================== HỌC PHẦN =============================== */
IF OBJECT_ID('dbo.MonHoc','U') IS NULL
CREATE TABLE dbo.MonHoc (
    MonHocId int IDENTITY(1,1) CONSTRAINT PK_MonHoc PRIMARY KEY,
    MaMon varchar(30) NOT NULL,
    TenMon nvarchar(200) NOT NULL,
    SoTinChi tinyint NOT NULL,
    SoTietLyThuyet smallint NOT NULL CONSTRAINT DF_MonHoc_LT DEFAULT 0,
    SoTietThucHanh smallint NOT NULL CONSTRAINT DF_MonHoc_TH DEFAULT 0,
    DonViPhuTrachId int NULL,
    LoaiMon varchar(20) NOT NULL CONSTRAINT DF_MonHoc_Loai DEFAULT 'BAT_BUOC',
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_MonHoc_TrangThai DEFAULT 'ACTIVE',
    CONSTRAINT UQ_MonHoc_Ma UNIQUE(MaMon),
    CONSTRAINT FK_MonHoc_DonVi FOREIGN KEY(DonViPhuTrachId) REFERENCES dbo.DonVi(DonViId),
    CONSTRAINT CK_MonHoc_TinChi CHECK(SoTinChi BETWEEN 0 AND 60),
    CONSTRAINT CK_MonHoc_Tiet CHECK(SoTietLyThuyet >= 0 AND SoTietThucHanh >= 0),
    CONSTRAINT CK_MonHoc_Loai CHECK(LoaiMon IN ('BAT_BUOC','TU_CHON','DIEU_KIEN','KHAC')),
    CONSTRAINT CK_MonHoc_TrangThai CHECK(TrangThai IN ('ACTIVE','INACTIVE'))
);
GO

IF OBJECT_ID('dbo.HocPhanTienQuyet','U') IS NULL
CREATE TABLE dbo.HocPhanTienQuyet (
    MonHocId int NOT NULL,
    MonTienQuyetId int NOT NULL,
    LoaiDieuKien varchar(20) NOT NULL CONSTRAINT DF_TienQuyet_Loai DEFAULT 'PASSED',
    CONSTRAINT PK_HocPhanTienQuyet PRIMARY KEY(MonHocId, MonTienQuyetId),
    CONSTRAINT FK_TienQuyet_Mon FOREIGN KEY(MonHocId) REFERENCES dbo.MonHoc(MonHocId),
    CONSTRAINT FK_TienQuyet_TienQuyet FOREIGN KEY(MonTienQuyetId) REFERENCES dbo.MonHoc(MonHocId),
    CONSTRAINT CK_TienQuyet_Khac CHECK(MonHocId <> MonTienQuyetId),
    CONSTRAINT CK_TienQuyet_Loai CHECK(LoaiDieuKien IN ('PASSED','COMPLETED'))
);
GO

IF OBJECT_ID('dbo.ChuongTrinhMonHoc','U') IS NULL
CREATE TABLE dbo.ChuongTrinhMonHoc (
    ChuongTrinhId int NOT NULL,
    MonHocId int NOT NULL,
    HocKyDuKien tinyint NULL,
    BatBuoc bit NOT NULL CONSTRAINT DF_CTMH_BatBuoc DEFAULT 1,
    GhiChu nvarchar(300) NULL,
    CONSTRAINT PK_ChuongTrinhMonHoc PRIMARY KEY(ChuongTrinhId, MonHocId),
    CONSTRAINT FK_CTMH_CTD FOREIGN KEY(ChuongTrinhId) REFERENCES dbo.ChuongTrinhDaoTao(ChuongTrinhId),
    CONSTRAINT FK_CTMH_Mon FOREIGN KEY(MonHocId) REFERENCES dbo.MonHoc(MonHocId),
    CONSTRAINT CK_CTMH_HocKy CHECK(HocKyDuKien IS NULL OR HocKyDuKien BETWEEN 1 AND 20)
);
GO

IF OBJECT_ID('dbo.PhongHoc','U') IS NULL
CREATE TABLE dbo.PhongHoc (
    PhongHocId int IDENTITY(1,1) CONSTRAINT PK_PhongHoc PRIMARY KEY,
    MaPhong varchar(30) NOT NULL,
    TenPhong nvarchar(100) NOT NULL,
    CoSo nvarchar(100) NULL,
    SucChua smallint NOT NULL,
    LoaiPhong varchar(20) NOT NULL CONSTRAINT DF_Phong_Loai DEFAULT 'LY_THUYET',
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_Phong_TrangThai DEFAULT 'ACTIVE',
    CONSTRAINT UQ_PhongHoc_Ma UNIQUE(MaPhong),
    CONSTRAINT CK_Phong_SucChua CHECK(SucChua > 0),
    CONSTRAINT CK_Phong_Loai CHECK(LoaiPhong IN ('LY_THUYET','THUC_HANH','HOI_TRUONG','KHAC')),
    CONSTRAINT CK_Phong_TrangThai CHECK(TrangThai IN ('ACTIVE','INACTIVE'))
);
GO

IF OBJECT_ID('dbo.LopHocPhan','U') IS NULL
CREATE TABLE dbo.LopHocPhan (
    LopHocPhanId int IDENTITY(1,1) CONSTRAINT PK_LopHocPhan PRIMARY KEY,
    MaLopHocPhan varchar(30) NOT NULL,
    MonHocId int NOT NULL,
    HocKyId int NOT NULL,
    GiangVienId int NOT NULL,
    Nhom varchar(20) NOT NULL CONSTRAINT DF_LHP_Nhom DEFAULT '01',
    SiSoToiDa smallint NOT NULL,
    NgayMoDangKy date NULL,
    NgayDongDangKy date NULL,
    TrangThai varchar(25) NOT NULL CONSTRAINT DF_LHP_TrangThai DEFAULT 'PLANNED',
    CONSTRAINT UQ_LHP_Ma UNIQUE(MaLopHocPhan),
    CONSTRAINT FK_LHP_Mon FOREIGN KEY(MonHocId) REFERENCES dbo.MonHoc(MonHocId),
    CONSTRAINT FK_LHP_HocKy FOREIGN KEY(HocKyId) REFERENCES dbo.HocKy(HocKyId),
    CONSTRAINT FK_LHP_GV FOREIGN KEY(GiangVienId) REFERENCES dbo.GiangVien(GiangVienId),
    CONSTRAINT CK_LHP_SiSo CHECK(SiSoToiDa > 0),
    CONSTRAINT CK_LHP_Han CHECK(NgayMoDangKy IS NULL OR NgayDongDangKy IS NULL OR NgayMoDangKy <= NgayDongDangKy),
    CONSTRAINT CK_LHP_TrangThai CHECK(TrangThai IN ('PLANNED','OPEN','IN_PROGRESS','FINISHED','CANCELLED'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LHP_HocKy_Mon' AND object_id=OBJECT_ID('dbo.LopHocPhan'))
    CREATE INDEX IX_LHP_HocKy_Mon ON dbo.LopHocPhan(HocKyId, MonHocId, TrangThai);
GO

IF OBJECT_ID('dbo.LichHoc','U') IS NULL
CREATE TABLE dbo.LichHoc (
    LichHocId int IDENTITY(1,1) CONSTRAINT PK_LichHoc PRIMARY KEY,
    LopHocPhanId int NOT NULL,
    PhongHocId int NOT NULL,
    Thu tinyint NOT NULL, -- 2=Thứ 2 ... 8=Chủ nhật; 1 không sử dụng
    TietBatDau tinyint NOT NULL,
    SoTiet tinyint NOT NULL,
    NgayBatDau date NOT NULL,
    NgayKetThuc date NOT NULL,
    GioBatDau time(0) NULL,
    GioKetThuc time(0) NULL,
    CONSTRAINT FK_LichHoc_LHP FOREIGN KEY(LopHocPhanId) REFERENCES dbo.LopHocPhan(LopHocPhanId),
    CONSTRAINT FK_LichHoc_Phong FOREIGN KEY(PhongHocId) REFERENCES dbo.PhongHoc(PhongHocId),
    CONSTRAINT CK_LichHoc_Thu CHECK(Thu BETWEEN 2 AND 8),
    CONSTRAINT CK_LichHoc_Tiet CHECK(TietBatDau > 0 AND SoTiet > 0 AND TietBatDau + SoTiet <= 20),
    CONSTRAINT CK_LichHoc_Ngay CHECK(NgayBatDau <= NgayKetThuc),
    CONSTRAINT CK_LichHoc_Gio CHECK(GioBatDau IS NULL OR GioKetThuc IS NULL OR GioBatDau < GioKetThuc)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LichHoc_LHP' AND object_id=OBJECT_ID('dbo.LichHoc'))
    CREATE INDEX IX_LichHoc_LHP ON dbo.LichHoc(LopHocPhanId);
GO

/* ========================= ĐĂNG KÝ VÀ KẾT QUẢ =========================== */
IF OBJECT_ID('dbo.DangKyHocPhan','U') IS NULL
CREATE TABLE dbo.DangKyHocPhan (
    DangKyId bigint IDENTITY(1,1) CONSTRAINT PK_DangKyHocPhan PRIMARY KEY,
    SinhVienId int NOT NULL,
    LopHocPhanId int NOT NULL,
    NgayDangKy datetime2(0) NOT NULL CONSTRAINT DF_DK_Ngay DEFAULT SYSUTCDATETIME(),
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_DK_TrangThai DEFAULT 'ENROLLED',
    LanHoc tinyint NOT NULL CONSTRAINT DF_DK_LanHoc DEFAULT 1,
    GhiChu nvarchar(300) NULL,
    RowVer rowversion,
    CONSTRAINT FK_DK_SV FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId),
    CONSTRAINT FK_DK_LHP FOREIGN KEY(LopHocPhanId) REFERENCES dbo.LopHocPhan(LopHocPhanId),
    CONSTRAINT UQ_DK_SV_LHP UNIQUE(SinhVienId, LopHocPhanId),
    CONSTRAINT CK_DK_TrangThai CHECK(TrangThai IN ('ENROLLED','DROPPED','COMPLETED','CANCELLED')),
    CONSTRAINT CK_DK_LanHoc CHECK(LanHoc >= 1)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_DK_SV_TrangThai' AND object_id=OBJECT_ID('dbo.DangKyHocPhan'))
    CREATE INDEX IX_DK_SV_TrangThai ON dbo.DangKyHocPhan(SinhVienId, TrangThai);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_DK_LHP_TrangThai' AND object_id=OBJECT_ID('dbo.DangKyHocPhan'))
    CREATE INDEX IX_DK_LHP_TrangThai ON dbo.DangKyHocPhan(LopHocPhanId, TrangThai);
GO

IF OBJECT_ID('dbo.ThanhPhanDiem','U') IS NULL
CREATE TABLE dbo.ThanhPhanDiem (
    ThanhPhanDiemId int IDENTITY(1,1) CONSTRAINT PK_ThanhPhanDiem PRIMARY KEY,
    LopHocPhanId int NOT NULL,
    MaThanhPhan varchar(30) NOT NULL,
    TenThanhPhan nvarchar(100) NOT NULL,
    TrongSo decimal(5,2) NOT NULL,
    DiemToiDa decimal(5,2) NOT NULL CONSTRAINT DF_TPD_DiemMax DEFAULT 10,
    BatBuoc bit NOT NULL CONSTRAINT DF_TPD_BatBuoc DEFAULT 1,
    CONSTRAINT FK_TPD_LHP FOREIGN KEY(LopHocPhanId) REFERENCES dbo.LopHocPhan(LopHocPhanId),
    CONSTRAINT UQ_TPD_LHP_Ma UNIQUE(LopHocPhanId, MaThanhPhan),
    CONSTRAINT CK_TPD_TrongSo CHECK(TrongSo > 0 AND TrongSo <= 100),
    CONSTRAINT CK_TPD_DiemMax CHECK(DiemToiDa > 0)
);
GO

IF OBJECT_ID('dbo.DiemThanhPhan','U') IS NULL
CREATE TABLE dbo.DiemThanhPhan (
    DangKyId bigint NOT NULL,
    ThanhPhanDiemId int NOT NULL,
    Diem decimal(5,2) NULL,
    NhanXet nvarchar(500) NULL,
    NguoiNhap nvarchar(450) NULL,
    NgayNhap datetime2(0) NULL,
    NgayCapNhat datetime2(0) NULL,
    CONSTRAINT PK_DiemThanhPhan PRIMARY KEY(DangKyId, ThanhPhanDiemId),
    CONSTRAINT FK_DTP_DK FOREIGN KEY(DangKyId) REFERENCES dbo.DangKyHocPhan(DangKyId),
    CONSTRAINT FK_DTP_TPD FOREIGN KEY(ThanhPhanDiemId) REFERENCES dbo.ThanhPhanDiem(ThanhPhanDiemId),
    CONSTRAINT CK_DTP_Diem CHECK(Diem IS NULL OR Diem >= 0)
);
GO

IF OBJECT_ID('dbo.KetQuaHocTap','U') IS NULL
CREATE TABLE dbo.KetQuaHocTap (
    DangKyId bigint NOT NULL CONSTRAINT PK_KetQuaHocTap PRIMARY KEY,
    DiemTongKet decimal(4,2) NULL,
    DiemChu varchar(2) NULL,
    DiemHe4 decimal(3,2) NULL,
    Dat bit NULL,
    TinChiTinhTichLuy tinyint NOT NULL CONSTRAINT DF_KQ_TinChi DEFAULT 0,
    NgayChot datetime2(0) NULL,
    NguoiChot nvarchar(450) NULL,
    PhienBan int NOT NULL CONSTRAINT DF_KQ_PhienBan DEFAULT 1,
    CONSTRAINT FK_KQ_DK FOREIGN KEY(DangKyId) REFERENCES dbo.DangKyHocPhan(DangKyId),
    CONSTRAINT CK_KQ_Diem CHECK(DiemTongKet IS NULL OR DiemTongKet BETWEEN 0 AND 10),
    CONSTRAINT CK_KQ_Diem4 CHECK(DiemHe4 IS NULL OR DiemHe4 BETWEEN 0 AND 4),
    CONSTRAINT CK_KQ_TinChi CHECK(TinChiTinhTichLuy >= 0)
);
GO

IF OBJECT_ID('dbo.LichSuChinhSuaDiem','U') IS NULL
CREATE TABLE dbo.LichSuChinhSuaDiem (
    LichSuDiemId bigint IDENTITY(1,1) CONSTRAINT PK_LichSuChinhSuaDiem PRIMARY KEY,
    DangKyId bigint NOT NULL,
    DiemCu decimal(4,2) NULL,
    DiemMoi decimal(4,2) NULL,
    LyDo nvarchar(500) NOT NULL,
    NguoiThayDoi nvarchar(450) NOT NULL,
    ThoiDiem datetime2(0) NOT NULL CONSTRAINT DF_LS_Diem_Time DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LS_Diem_DK FOREIGN KEY(DangKyId) REFERENCES dbo.DangKyHocPhan(DangKyId)
);
GO

IF OBJECT_ID('dbo.BuoiDiemDanh','U') IS NULL
CREATE TABLE dbo.BuoiDiemDanh (
    BuoiDiemDanhId bigint IDENTITY(1,1) CONSTRAINT PK_BuoiDiemDanh PRIMARY KEY,
    LopHocPhanId int NOT NULL,
    ThoiGianBatDau datetime2(0) NOT NULL,
    ThoiGianKetThuc datetime2(0) NULL,
    NoiDung nvarchar(200) NULL,
    NguoiTao nvarchar(450) NULL,
    CONSTRAINT FK_BDD_LHP FOREIGN KEY(LopHocPhanId) REFERENCES dbo.LopHocPhan(LopHocPhanId),
    CONSTRAINT CK_BDD_Gio CHECK(ThoiGianKetThuc IS NULL OR ThoiGianBatDau < ThoiGianKetThuc)
);
GO

IF OBJECT_ID('dbo.DiemDanh','U') IS NULL
CREATE TABLE dbo.DiemDanh (
    BuoiDiemDanhId bigint NOT NULL,
    SinhVienId int NOT NULL,
    TrangThai varchar(20) NOT NULL,
    GhiChu nvarchar(300) NULL,
    NguoiCapNhat nvarchar(450) NULL,
    CapNhatLuc datetime2(0) NOT NULL CONSTRAINT DF_DiemDanh_Time DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_DiemDanh PRIMARY KEY(BuoiDiemDanhId, SinhVienId),
    CONSTRAINT FK_DiemDanh_Buoi FOREIGN KEY(BuoiDiemDanhId) REFERENCES dbo.BuoiDiemDanh(BuoiDiemDanhId),
    CONSTRAINT FK_DiemDanh_SV FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId),
    CONSTRAINT CK_DiemDanh_TrangThai CHECK(TrangThai IN ('PRESENT','LATE','ABSENT','EXCUSED'))
);
GO

/* ============================ TÀI CHÍNH ================================== */
IF OBJECT_ID('dbo.BieuPhiHocPhi','U') IS NULL
CREATE TABLE dbo.BieuPhiHocPhi (
    BieuPhiId int IDENTITY(1,1) CONSTRAINT PK_BieuPhiHocPhi PRIMARY KEY,
    ChuongTrinhId int NULL,
    NamHocId int NOT NULL,
    DonGiaTinChi decimal(18,2) NOT NULL,
    LoaiTien char(3) NOT NULL CONSTRAINT DF_BieuPhi_Tien DEFAULT 'VND',
    HieuLucTu date NOT NULL,
    HieuLucDen date NULL,
    CONSTRAINT FK_BieuPhi_CTD FOREIGN KEY(ChuongTrinhId) REFERENCES dbo.ChuongTrinhDaoTao(ChuongTrinhId),
    CONSTRAINT FK_BieuPhi_NamHoc FOREIGN KEY(NamHocId) REFERENCES dbo.NamHoc(NamHocId),
    CONSTRAINT CK_BieuPhi_Gia CHECK(DonGiaTinChi >= 0),
    CONSTRAINT CK_BieuPhi_Ngay CHECK(HieuLucDen IS NULL OR HieuLucTu <= HieuLucDen)
);
GO

IF OBJECT_ID('dbo.HoaDonHocPhi','U') IS NULL
CREATE TABLE dbo.HoaDonHocPhi (
    HoaDonId bigint IDENTITY(1,1) CONSTRAINT PK_HoaDonHocPhi PRIMARY KEY,
    SoHoaDon varchar(40) NOT NULL,
    SinhVienId int NOT NULL,
    HocKyId int NOT NULL,
    NgayLap datetime2(0) NOT NULL CONSTRAINT DF_HoaDon_Ngay DEFAULT SYSUTCDATETIME(),
    HanThanhToan date NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_HoaDon_TrangThai DEFAULT 'ISSUED',
    GhiChu nvarchar(500) NULL,
    CONSTRAINT UQ_HoaDon_So UNIQUE(SoHoaDon),
    CONSTRAINT FK_HoaDon_SV FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId),
    CONSTRAINT FK_HoaDon_HocKy FOREIGN KEY(HocKyId) REFERENCES dbo.HocKy(HocKyId),
    CONSTRAINT CK_HoaDon_TrangThai CHECK(TrangThai IN ('DRAFT','ISSUED','PARTIAL','PAID','CANCELLED'))
);
GO

IF OBJECT_ID('dbo.ChiTietHoaDonHocPhi','U') IS NULL
CREATE TABLE dbo.ChiTietHoaDonHocPhi (
    ChiTietHoaDonId bigint IDENTITY(1,1) CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY,
    HoaDonId bigint NOT NULL,
    LoaiKhoanThu varchar(30) NOT NULL,
    MoTa nvarchar(300) NOT NULL,
    SoLuong decimal(10,2) NOT NULL CONSTRAINT DF_CTHD_SL DEFAULT 1,
    DonGia decimal(18,2) NOT NULL,
    SoTienGiam decimal(18,2) NOT NULL CONSTRAINT DF_CTHD_Giam DEFAULT 0,
    CONSTRAINT FK_CTHD_HD FOREIGN KEY(HoaDonId) REFERENCES dbo.HoaDonHocPhi(HoaDonId),
    CONSTRAINT CK_CTHD_SL CHECK(SoLuong > 0),
    CONSTRAINT CK_CTHD_Gia CHECK(DonGia >= 0 AND SoTienGiam >= 0 AND SoTienGiam <= SoLuong * DonGia),
    CONSTRAINT CK_CTHD_Loai CHECK(LoaiKhoanThu IN ('TIN_CHI','LE_PHI','KTX','BAO_HIEM','KHAC'))
);
GO

IF OBJECT_ID('dbo.ThanhToan','U') IS NULL
CREATE TABLE dbo.ThanhToan (
    ThanhToanId bigint IDENTITY(1,1) CONSTRAINT PK_ThanhToan PRIMARY KEY,
    HoaDonId bigint NOT NULL,
    MaGiaoDich varchar(100) NULL,
    SoTien decimal(18,2) NOT NULL,
    ThoiGianThanhToan datetime2(0) NOT NULL CONSTRAINT DF_ThanhToan_Time DEFAULT SYSUTCDATETIME(),
    PhuongThuc varchar(20) NOT NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_ThanhToan_TrangThai DEFAULT 'PENDING',
    NguonThanhToan nvarchar(100) NULL,
    IdempotencyKey varchar(100) NULL,
    GhiChu nvarchar(500) NULL,
    CONSTRAINT FK_ThanhToan_HD FOREIGN KEY(HoaDonId) REFERENCES dbo.HoaDonHocPhi(HoaDonId),
    CONSTRAINT CK_ThanhToan_SoTien CHECK(SoTien > 0),
    CONSTRAINT CK_ThanhToan_PhuongThuc CHECK(PhuongThuc IN ('CASH','BANK_TRANSFER','CARD','E_WALLET','OTHER')),
    CONSTRAINT CK_ThanhToan_TrangThai CHECK(TrangThai IN ('PENDING','SUCCEEDED','FAILED','REFUNDED')),
    CONSTRAINT UQ_ThanhToan_Idempotency UNIQUE(IdempotencyKey)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ThanhToan_HD_TrangThai' AND object_id=OBJECT_ID('dbo.ThanhToan'))
    CREATE INDEX IX_ThanhToan_HD_TrangThai ON dbo.ThanhToan(HoaDonId, TrangThai);
GO

/* ============================ HỌC BỔNG / KỶ LUẬT ========================= */
IF OBJECT_ID('dbo.ChuongTrinhHocBong','U') IS NULL
CREATE TABLE dbo.ChuongTrinhHocBong (
    HocBongId int IDENTITY(1,1) CONSTRAINT PK_ChuongTrinhHocBong PRIMARY KEY,
    MaHocBong varchar(30) NOT NULL,
    TenHocBong nvarchar(200) NOT NULL,
    NamHocId int NOT NULL,
    SoTienToiDa decimal(18,2) NULL,
    TieuChi nvarchar(max) NULL,
    NgayBatDau date NULL,
    NgayKetThuc date NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_HocBong_TrangThai DEFAULT 'OPEN',
    CONSTRAINT UQ_HocBong_Ma UNIQUE(MaHocBong),
    CONSTRAINT FK_HocBong_NamHoc FOREIGN KEY(NamHocId) REFERENCES dbo.NamHoc(NamHocId),
    CONSTRAINT CK_HocBong_Tien CHECK(SoTienToiDa IS NULL OR SoTienToiDa >= 0),
    CONSTRAINT CK_HocBong_Ngay CHECK(NgayBatDau IS NULL OR NgayKetThuc IS NULL OR NgayBatDau <= NgayKetThuc),
    CONSTRAINT CK_HocBong_TrangThai CHECK(TrangThai IN ('DRAFT','OPEN','CLOSED'))
);
GO

IF OBJECT_ID('dbo.CapHocBong','U') IS NULL
CREATE TABLE dbo.CapHocBong (
    CapHocBongId bigint IDENTITY(1,1) CONSTRAINT PK_CapHocBong PRIMARY KEY,
    HocBongId int NOT NULL,
    SinhVienId int NOT NULL,
    SoTien decimal(18,2) NOT NULL,
    NgayQuyetDinh date NOT NULL,
    SoQuyetDinh varchar(50) NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_CapHocBong_TrangThai DEFAULT 'APPROVED',
    GhiChu nvarchar(500) NULL,
    CONSTRAINT FK_CapHB_HB FOREIGN KEY(HocBongId) REFERENCES dbo.ChuongTrinhHocBong(HocBongId),
    CONSTRAINT FK_CapHB_SV FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId),
    CONSTRAINT UQ_CapHB_HB_SV UNIQUE(HocBongId, SinhVienId),
    CONSTRAINT CK_CapHB_Tien CHECK(SoTien >= 0),
    CONSTRAINT CK_CapHB_TrangThai CHECK(TrangThai IN ('PENDING','APPROVED','REJECTED','PAID'))
);
GO

IF OBJECT_ID('dbo.HoSoSinhVien','U') IS NULL
CREATE TABLE dbo.HoSoSinhVien (
    HoSoId bigint IDENTITY(1,1) CONSTRAINT PK_HoSoSinhVien PRIMARY KEY,
    SinhVienId int NOT NULL,
    LoaiHoSo varchar(40) NOT NULL,
    TenTep nvarchar(255) NOT NULL,
    UriLuuTru nvarchar(1000) NOT NULL, -- URL/key lưu ở storage, không lưu file nhị phân lớn trong bảng
    MimeType varchar(100) NULL,
    KichThuocBytes bigint NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_HoSo_TrangThai DEFAULT 'PENDING',
    NguoiDuyet nvarchar(450) NULL,
    NgayDuyet datetime2(0) NULL,
    GhiChu nvarchar(500) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_HoSo_Created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_HoSo_SV FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId),
    CONSTRAINT CK_HoSo_KichThuoc CHECK(KichThuocBytes IS NULL OR KichThuocBytes >= 0),
    CONSTRAINT CK_HoSo_TrangThai CHECK(TrangThai IN ('PENDING','APPROVED','REJECTED'))
);
GO

IF OBJECT_ID('dbo.KyLuatSinhVien','U') IS NULL
CREATE TABLE dbo.KyLuatSinhVien (
    KyLuatId bigint IDENTITY(1,1) CONSTRAINT PK_KyLuatSinhVien PRIMARY KEY,
    SinhVienId int NOT NULL,
    LoaiViPham varchar(30) NOT NULL,
    MoTa nvarchar(1000) NOT NULL,
    SoQuyetDinh varchar(50) NULL,
    NgayQuyetDinh date NOT NULL,
    NgayHetHieuLuc date NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_KyLuat_TrangThai DEFAULT 'ACTIVE',
    NguoiLap nvarchar(450) NULL,
    CONSTRAINT FK_KyLuat_SV FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId),
    CONSTRAINT CK_KyLuat_Ngay CHECK(NgayHetHieuLuc IS NULL OR NgayQuyetDinh <= NgayHetHieuLuc),
    CONSTRAINT CK_KyLuat_TrangThai CHECK(TrangThai IN ('ACTIVE','EXPIRED','REVOKED'))
);
GO

/* ============================== KIỂM TOÁN ================================ */
IF OBJECT_ID('dbo.NhatKyKiemToan','U') IS NULL
CREATE TABLE dbo.NhatKyKiemToan (
    NhatKyId bigint IDENTITY(1,1) CONSTRAINT PK_NhatKyKiemToan PRIMARY KEY,
    UserId nvarchar(450) NULL,
    HanhDong varchar(50) NOT NULL,
    TenBang varchar(128) NOT NULL,
    KhoaBanGhi nvarchar(200) NULL,
    ThoiDiem datetime2(0) NOT NULL CONSTRAINT DF_Audit_Time DEFAULT SYSUTCDATETIME(),
    IpAddress varchar(45) NULL,
    CorrelationId varchar(100) NULL,
    DuLieuCu nvarchar(max) NULL,
    DuLieuMoi nvarchar(max) NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Audit_Bang_ThoiDiem' AND object_id=OBJECT_ID('dbo.NhatKyKiemToan'))
    CREATE INDEX IX_Audit_Bang_ThoiDiem ON dbo.NhatKyKiemToan(TenBang, ThoiDiem DESC);
GO

/* ================================ VIEWS ================================== */
CREATE OR ALTER VIEW dbo.vw_DanhBaSinhVien
AS
SELECT sv.SinhVienId, sv.MaSinhVien,
       CONCAT(sv.Ho, N' ', NULLIF(sv.TenDem,N''), CASE WHEN sv.TenDem IS NULL OR sv.TenDem=N'' THEN N'' ELSE N' ' END, sv.Ten) AS HoTen,
       sv.NgaySinh, sv.GioiTinh, sv.EmailTruong, sv.SoDienThoai, sv.TrangThai,
       lh.MaLop, lh.TenLop, n.MaNganh, n.TenNganh, ctd.TenChuongTrinh
FROM dbo.SinhVien sv
JOIN dbo.LopHanhChinh lh ON lh.LopId=sv.LopId
JOIN dbo.Nganh n ON n.NganhId=lh.NganhId
JOIN dbo.ChuongTrinhDaoTao ctd ON ctd.ChuongTrinhId=lh.ChuongTrinhId;
GO

CREATE OR ALTER VIEW dbo.vw_DangKyHocPhanChiTiet
AS
SELECT dk.DangKyId, sv.MaSinhVien,
       CONCAT(sv.Ho,N' ',COALESCE(sv.TenDem+N' ',N''),sv.Ten) AS HoTen,
       lhp.MaLopHocPhan, mh.MaMon, mh.TenMon, mh.SoTinChi,
       hk.TenHocKy, nh.MaNamHoc, dk.LanHoc, dk.TrangThai AS TrangThaiDangKy,
       kq.DiemTongKet, kq.DiemChu, kq.DiemHe4, kq.Dat, kq.TinChiTinhTichLuy
FROM dbo.DangKyHocPhan dk
JOIN dbo.SinhVien sv ON sv.SinhVienId=dk.SinhVienId
JOIN dbo.LopHocPhan lhp ON lhp.LopHocPhanId=dk.LopHocPhanId
JOIN dbo.MonHoc mh ON mh.MonHocId=lhp.MonHocId
JOIN dbo.HocKy hk ON hk.HocKyId=lhp.HocKyId
JOIN dbo.NamHoc nh ON nh.NamHocId=hk.NamHocId
LEFT JOIN dbo.KetQuaHocTap kq ON kq.DangKyId=dk.DangKyId;
GO

CREATE OR ALTER VIEW dbo.vw_CongNoHocPhi
AS
WITH TongHoaDon AS (
    SELECT hd.HoaDonId, hd.SoHoaDon, hd.SinhVienId, hd.HocKyId, hd.TrangThai,
           SUM(ct.SoLuong*ct.DonGia-ct.SoTienGiam) AS TongTien
    FROM dbo.HoaDonHocPhi hd
    LEFT JOIN dbo.ChiTietHoaDonHocPhi ct ON ct.HoaDonId=hd.HoaDonId
    GROUP BY hd.HoaDonId, hd.SoHoaDon, hd.SinhVienId, hd.HocKyId, hd.TrangThai
),
TongThanhToan AS (
    SELECT HoaDonId, SUM(CASE WHEN TrangThai='SUCCEEDED' THEN SoTien ELSE 0 END) AS DaThanhToan
    FROM dbo.ThanhToan GROUP BY HoaDonId
)
SELECT thd.HoaDonId, thd.SoHoaDon, sv.MaSinhVien,
       CONCAT(sv.Ho,N' ',COALESCE(sv.TenDem+N' ',N''),sv.Ten) AS HoTen,
       hk.TenHocKy, thd.TrangThai, thd.TongTien,
       COALESCE(tt.DaThanhToan,0) AS DaThanhToan,
       thd.TongTien-COALESCE(tt.DaThanhToan,0) AS ConNo
FROM TongHoaDon thd
JOIN dbo.SinhVien sv ON sv.SinhVienId=thd.SinhVienId
JOIN dbo.HocKy hk ON hk.HocKyId=thd.HocKyId
LEFT JOIN TongThanhToan tt ON tt.HoaDonId=thd.HoaDonId;
GO

CREATE OR ALTER VIEW dbo.vw_GPAHocKy
AS
SELECT sv.SinhVienId, sv.MaSinhVien, hk.HocKyId, hk.TenHocKy,
       CAST(SUM(CASE WHEN kq.Dat IS NOT NULL THEN COALESCE(kq.DiemHe4,0)*mh.SoTinChi ELSE 0 END)
            / NULLIF(SUM(CASE WHEN kq.Dat IS NOT NULL THEN mh.SoTinChi ELSE 0 END),0) AS decimal(4,2)) AS GPAHe4,
       CAST(SUM(CASE WHEN kq.DiemTongKet IS NOT NULL THEN kq.DiemTongKet*mh.SoTinChi ELSE 0 END)
            / NULLIF(SUM(CASE WHEN kq.DiemTongKet IS NOT NULL THEN mh.SoTinChi ELSE 0 END),0) AS decimal(4,2)) AS DiemTBHe10,
       SUM(CASE WHEN kq.Dat=1 THEN mh.SoTinChi ELSE 0 END) AS TinChiDat
FROM dbo.DangKyHocPhan dk
JOIN dbo.SinhVien sv ON sv.SinhVienId=dk.SinhVienId
JOIN dbo.LopHocPhan lhp ON lhp.LopHocPhanId=dk.LopHocPhanId
JOIN dbo.HocKy hk ON hk.HocKyId=lhp.HocKyId
JOIN dbo.MonHoc mh ON mh.MonHocId=lhp.MonHocId
LEFT JOIN dbo.KetQuaHocTap kq ON kq.DangKyId=dk.DangKyId
WHERE dk.TrangThai IN ('ENROLLED','COMPLETED')
GROUP BY sv.SinhVienId, sv.MaSinhVien, hk.HocKyId, hk.TenHocKy;
GO

/* =========================== STORED PROCEDURES =========================== */
/*
Đăng ký học phần có kiểm tra trạng thái, học kỳ, sĩ số, trùng đăng ký,
điều kiện tiên quyết và lịch học trùng của sinh viên.
Lưu ý: chính sách "học lại / học cải thiện" cần được cấu hình theo quy chế trường.
*/
CREATE OR ALTER PROCEDURE dbo.usp_DangKyHocPhan
    @SinhVienId int,
    @LopHocPhanId int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRAN;

        DECLARE @TrangThaiSV varchar(25), @TrangThaiLHP varchar(25),
                @HocKyId int, @MonHocId int, @SiSoToiDa int,
                @NgayMo date, @NgayDong date, @TrangThaiHocKy varchar(20),
                @Today date = CONVERT(date, GETDATE());

        SELECT @TrangThaiSV=TrangThai FROM dbo.SinhVien WITH (UPDLOCK,HOLDLOCK)
        WHERE SinhVienId=@SinhVienId;
        IF @TrangThaiSV IS NULL THROW 51001, N'Không tìm thấy sinh viên.', 1;
        IF @TrangThaiSV <> 'STUDYING' THROW 51002, N'Sinh viên không ở trạng thái được phép đăng ký.', 1;

        SELECT @TrangThaiLHP=lhp.TrangThai, @HocKyId=lhp.HocKyId,
               @MonHocId=lhp.MonHocId, @SiSoToiDa=lhp.SiSoToiDa,
               @NgayMo=lhp.NgayMoDangKy, @NgayDong=lhp.NgayDongDangKy,
               @TrangThaiHocKy=hk.TrangThai
        FROM dbo.LopHocPhan lhp WITH (UPDLOCK,HOLDLOCK)
        JOIN dbo.HocKy hk ON hk.HocKyId=lhp.HocKyId
        WHERE lhp.LopHocPhanId=@LopHocPhanId;

        IF @HocKyId IS NULL THROW 51003, N'Không tìm thấy lớp học phần.', 1;
        IF @TrangThaiLHP <> 'OPEN' OR @TrangThaiHocKy <> 'REGISTRATION'
            THROW 51004, N'Lớp học phần hoặc học kỳ không mở đăng ký.', 1;
        IF @NgayMo IS NOT NULL AND @Today < @NgayMo
            THROW 51005, N'Chưa đến thời gian đăng ký.', 1;
        IF @NgayDong IS NOT NULL AND @Today > @NgayDong
            THROW 51006, N'Đã hết thời gian đăng ký.', 1;
        IF EXISTS (SELECT 1 FROM dbo.DangKyHocPhan WHERE SinhVienId=@SinhVienId AND LopHocPhanId=@LopHocPhanId AND TrangThai='ENROLLED')
            THROW 51007, N'Sinh viên đã đăng ký lớp học phần này.', 1;

        IF (SELECT COUNT(*) FROM dbo.DangKyHocPhan WITH (UPDLOCK,HOLDLOCK)
            WHERE LopHocPhanId=@LopHocPhanId AND TrangThai='ENROLLED') >= @SiSoToiDa
            THROW 51008, N'Lớp học phần đã đủ sĩ số.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.HocPhanTienQuyet tq
            WHERE tq.MonHocId=@MonHocId
              AND tq.LoaiDieuKien='PASSED'
              AND NOT EXISTS (
                SELECT 1 FROM dbo.DangKyHocPhan dk
                JOIN dbo.LopHocPhan lhp0 ON lhp0.LopHocPhanId=dk.LopHocPhanId
                JOIN dbo.KetQuaHocTap kq ON kq.DangKyId=dk.DangKyId
                WHERE dk.SinhVienId=@SinhVienId AND lhp0.MonHocId=tq.MonTienQuyetId
                  AND kq.Dat=1 AND dk.TrangThai='COMPLETED'
              )
        ) THROW 51009, N'Sinh viên chưa đạt môn học tiên quyết.', 1;

        IF EXISTS (
            SELECT 1
            FROM dbo.LichHoc newSch
            JOIN dbo.LichHoc oldSch ON oldSch.Thu=newSch.Thu
              AND oldSch.NgayBatDau <= newSch.NgayKetThuc
              AND newSch.NgayBatDau <= oldSch.NgayKetThuc
              AND oldSch.TietBatDau < newSch.TietBatDau + newSch.SoTiet
              AND newSch.TietBatDau < oldSch.TietBatDau + oldSch.SoTiet
            JOIN dbo.DangKyHocPhan dk ON dk.LopHocPhanId=oldSch.LopHocPhanId
            WHERE newSch.LopHocPhanId=@LopHocPhanId
              AND dk.SinhVienId=@SinhVienId AND dk.TrangThai='ENROLLED'
        ) THROW 51010, N'Lịch học bị trùng với lớp học phần đã đăng ký.', 1;

        INSERT dbo.DangKyHocPhan(SinhVienId,LopHocPhanId,TrangThai,LanHoc)
        VALUES(@SinhVienId,@LopHocPhanId,'ENROLLED',
            1 + (SELECT COUNT(*) FROM dbo.DangKyHocPhan dk
                 JOIN dbo.LopHocPhan oldLhp ON oldLhp.LopHocPhanId=dk.LopHocPhanId
                 WHERE dk.SinhVienId=@SinhVienId AND oldLhp.MonHocId=@MonHocId));

        COMMIT;
        SELECT CAST(SCOPE_IDENTITY() AS bigint) AS DangKyId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END;
GO

/* Chốt điểm tổng kết từ các thành phần; tổng trọng số phải bằng 100. */
CREATE OR ALTER PROCEDURE dbo.usp_ChotDiem
    @DangKyId bigint,
    @NguoiChot nvarchar(450),
    @DiemDat decimal(4,2) = 4.00
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @LopHocPhanId int, @TongTrongSo decimal(8,2), @Diem decimal(8,4), @TinChi tinyint;
    SELECT @LopHocPhanId=dk.LopHocPhanId, @TinChi=mh.SoTinChi
    FROM dbo.DangKyHocPhan dk
    JOIN dbo.LopHocPhan lhp ON lhp.LopHocPhanId=dk.LopHocPhanId
    JOIN dbo.MonHoc mh ON mh.MonHocId=lhp.MonHocId
    WHERE dk.DangKyId=@DangKyId AND dk.TrangThai IN ('ENROLLED','COMPLETED');
    IF @LopHocPhanId IS NULL THROW 51101, N'Không tìm thấy đăng ký học phần hợp lệ.', 1;

    SELECT @TongTrongSo=SUM(TrongSo)
    FROM dbo.ThanhPhanDiem WHERE LopHocPhanId=@LopHocPhanId;
    IF @TongTrongSo IS NULL OR ABS(@TongTrongSo-100)>0.01
        THROW 51102, N'Tổng trọng số các thành phần điểm phải bằng 100%.', 1;

    IF EXISTS (
        SELECT 1 FROM dbo.ThanhPhanDiem t
        LEFT JOIN dbo.DiemThanhPhan d ON d.ThanhPhanDiemId=t.ThanhPhanDiemId AND d.DangKyId=@DangKyId
        WHERE t.LopHocPhanId=@LopHocPhanId AND t.BatBuoc=1 AND d.Diem IS NULL
    ) THROW 51103, N'Chưa nhập đủ các thành phần điểm bắt buộc.', 1;

    IF EXISTS (
        SELECT 1 FROM dbo.DiemThanhPhan d
        JOIN dbo.ThanhPhanDiem t ON t.ThanhPhanDiemId=d.ThanhPhanDiemId
        WHERE d.DangKyId=@DangKyId AND d.Diem > t.DiemToiDa
    ) THROW 51104, N'Có điểm thành phần vượt điểm tối đa.', 1;

    SELECT @Diem=SUM(CAST(d.Diem/t.DiemToiDa*10.0*t.TrongSo/100.0 AS decimal(12,6)))
    FROM dbo.ThanhPhanDiem t
    JOIN dbo.DiemThanhPhan d ON d.ThanhPhanDiemId=t.ThanhPhanDiemId
    WHERE t.LopHocPhanId=@LopHocPhanId AND d.DangKyId=@DangKyId;

    SET @Diem=ROUND(@Diem,2);

    MERGE dbo.KetQuaHocTap AS target
    USING (SELECT @DangKyId AS DangKyId) AS source
    ON target.DangKyId=source.DangKyId
    WHEN MATCHED THEN UPDATE SET
        DiemTongKet=@Diem,
        DiemChu=CASE WHEN @Diem>=8.5 THEN 'A' WHEN @Diem>=7.0 THEN 'B' WHEN @Diem>=5.5 THEN 'C' WHEN @Diem>=4.0 THEN 'D' ELSE 'F' END,
        DiemHe4=CASE WHEN @Diem>=8.5 THEN 4.0 WHEN @Diem>=7.0 THEN 3.0 WHEN @Diem>=5.5 THEN 2.0 WHEN @Diem>=4.0 THEN 1.0 ELSE 0.0 END,
        Dat=CASE WHEN @Diem>=@DiemDat THEN 1 ELSE 0 END,
        TinChiTinhTichLuy=CASE WHEN @Diem>=@DiemDat THEN @TinChi ELSE 0 END,
        NgayChot=SYSUTCDATETIME(), NguoiChot=@NguoiChot, PhienBan=PhienBan+1
    WHEN NOT MATCHED THEN INSERT(DangKyId,DiemTongKet,DiemChu,DiemHe4,Dat,TinChiTinhTichLuy,NgayChot,NguoiChot)
        VALUES(@DangKyId,@Diem,
        CASE WHEN @Diem>=8.5 THEN 'A' WHEN @Diem>=7.0 THEN 'B' WHEN @Diem>=5.5 THEN 'C' WHEN @Diem>=4.0 THEN 'D' ELSE 'F' END,
        CASE WHEN @Diem>=8.5 THEN 4.0 WHEN @Diem>=7.0 THEN 3.0 WHEN @Diem>=5.5 THEN 2.0 WHEN @Diem>=4.0 THEN 1.0 ELSE 0.0 END,
        CASE WHEN @Diem>=@DiemDat THEN 1 ELSE 0 END,
        CASE WHEN @Diem>=@DiemDat THEN @TinChi ELSE 0 END,SYSUTCDATETIME(),@NguoiChot);

    UPDATE dbo.DangKyHocPhan SET TrangThai='COMPLETED' WHERE DangKyId=@DangKyId;
    SELECT * FROM dbo.KetQuaHocTap WHERE DangKyId=@DangKyId;
END;
GO

/* Cập nhật trạng thái hóa đơn theo tổng tiền đã thu thành công. */
CREATE OR ALTER PROCEDURE dbo.usp_CapNhatTrangThaiHoaDon
    @HoaDonId bigint
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Tong decimal(18,2), @DaThu decimal(18,2);
    SELECT @Tong=COALESCE(SUM(SoLuong*DonGia-SoTienGiam),0)
    FROM dbo.ChiTietHoaDonHocPhi WHERE HoaDonId=@HoaDonId;
    SELECT @DaThu=COALESCE(SUM(CASE WHEN TrangThai='SUCCEEDED' THEN SoTien ELSE 0 END),0)
    FROM dbo.ThanhToan WHERE HoaDonId=@HoaDonId;
    IF NOT EXISTS(SELECT 1 FROM dbo.HoaDonHocPhi WHERE HoaDonId=@HoaDonId)
        THROW 51201, N'Không tìm thấy hóa đơn.', 1;
    UPDATE dbo.HoaDonHocPhi
    SET TrangThai=CASE WHEN TrangThai='CANCELLED' THEN 'CANCELLED'
                       WHEN @Tong=0 THEN 'ISSUED'
                       WHEN @DaThu<=0 THEN 'ISSUED'
                       WHEN @DaThu<@Tong THEN 'PARTIAL'
                       ELSE 'PAID' END
    WHERE HoaDonId=@HoaDonId;
END;
GO

/* ============================ DỮ LIỆU KHỞI TẠO =========================== */
IF NOT EXISTS(SELECT 1 FROM dbo.DonVi WHERE MaDonVi='UTC')
    INSERT dbo.DonVi(MaDonVi,TenDonVi,LoaiDonVi) VALUES('UTC',N'Trường Đại học Giao thông Vận tải','TRUONG');
GO
/* Chỉ là dữ liệu khởi tạo kỹ thuật; bổ sung danh mục chính thức theo trường. */

/* ============================ TRUY VẤN MẪU =============================== */
-- Danh bạ sinh viên:
-- SELECT * FROM dbo.vw_DanhBaSinhVien ORDER BY MaSinhVien;
-- Kết quả học tập:
-- SELECT * FROM dbo.vw_DangKyHocPhanChiTiet WHERE MaSinhVien='SV000001';
-- GPA học kỳ:
-- SELECT * FROM dbo.vw_GPAHocKy WHERE MaSinhVien='SV000001';
-- Công nợ học phí:
-- SELECT * FROM dbo.vw_CongNoHocPhi WHERE ConNo > 0;
-- Đăng ký học phần (chỉ gọi qua stored procedure để kiểm tra điều kiện):
-- EXEC dbo.usp_DangKyHocPhan @SinhVienId=1, @LopHocPhanId=1;
-- Chốt điểm:
-- EXEC dbo.usp_ChotDiem @DangKyId=1, @NguoiChot=N'admin';
GO

/* ========================= TAI KHOAN VA PHAN QUYEN ====================== */
/*
   PasswordHash chỉ chứa chuỗi băm do ứng dụng tạo (ví dụ ASP.NET Core
   PasswordHasher / ASP.NET Core Identity). Tuyệt đối không lưu mật khẩu rõ.
   Các quyền chi tiết vẫn phải được kiểm tra ở backend/API; không tin role do
   client gửi lên.
*/
IF OBJECT_ID('dbo.NhanVienPGV','U') IS NULL
CREATE TABLE dbo.NhanVienPGV (
    NhanVienPGVId int IDENTITY(1,1) CONSTRAINT PK_NhanVienPGV PRIMARY KEY,
    MaNhanVien varchar(20) NOT NULL,
    HoTen nvarchar(150) NOT NULL,
    Email varchar(254) NULL,
    SoDienThoai varchar(20) NULL,
    DonViId int NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_NhanVienPGV_TrangThai DEFAULT 'ACTIVE',
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_NhanVienPGV_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_NhanVienPGV_Ma UNIQUE(MaNhanVien),
    CONSTRAINT FK_NhanVienPGV_DonVi FOREIGN KEY(DonViId) REFERENCES dbo.DonVi(DonViId),
    CONSTRAINT CK_NhanVienPGV_TrangThai CHECK(TrangThai IN ('ACTIVE','INACTIVE'))
);
GO

IF OBJECT_ID('dbo.TaiKhoan','U') IS NULL
CREATE TABLE dbo.TaiKhoan (
    TaiKhoanId int IDENTITY(1,1) CONSTRAINT PK_TaiKhoan PRIMARY KEY,
    TenDangNhap varchar(100) NOT NULL,
    Email varchar(254) NULL,
    PasswordHash nvarchar(500) NOT NULL,
    HoTenHienThi nvarchar(150) NOT NULL,
    TrangThai varchar(20) NOT NULL CONSTRAINT DF_TaiKhoan_TrangThai DEFAULT 'ACTIVE',
    BuocDoiMatKhau bit NOT NULL CONSTRAINT DF_TaiKhoan_BuocDoiMK DEFAULT 1,
    SoLanDangNhapSai smallint NOT NULL CONSTRAINT DF_TaiKhoan_LoginFail DEFAULT 0,
    KhoaDen datetime2(0) NULL,
    LanDangNhapCuoi datetime2(0) NULL,
    CreatedAt datetime2(0) NOT NULL CONSTRAINT DF_TaiKhoan_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt datetime2(0) NULL,
    RowVer rowversion,
    CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE(TenDangNhap),
    CONSTRAINT CK_TaiKhoan_TrangThai CHECK(TrangThai IN ('ACTIVE','LOCKED','DISABLED')),
    CONSTRAINT CK_TaiKhoan_LoginFail CHECK(SoLanDangNhapSai >= 0)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_TaiKhoan_Email' AND object_id=OBJECT_ID('dbo.TaiKhoan'))
    CREATE UNIQUE INDEX UX_TaiKhoan_Email ON dbo.TaiKhoan(Email) WHERE Email IS NOT NULL;
GO

IF OBJECT_ID('dbo.VaiTro','U') IS NULL
CREATE TABLE dbo.VaiTro (
    VaiTroId tinyint IDENTITY(1,1) CONSTRAINT PK_VaiTro PRIMARY KEY,
    MaVaiTro varchar(20) NOT NULL,
    TenVaiTro nvarchar(100) NOT NULL,
    MoTa nvarchar(300) NULL,
    CONSTRAINT UQ_VaiTro_Ma UNIQUE(MaVaiTro),
    CONSTRAINT CK_VaiTro_Ma CHECK(MaVaiTro IN ('ADMIN','PGV','GV','HS'))
);
GO

IF OBJECT_ID('dbo.TaiKhoanVaiTro','U') IS NULL
CREATE TABLE dbo.TaiKhoanVaiTro (
    TaiKhoanId int NOT NULL,
    VaiTroId tinyint NOT NULL,
    NgayGan datetime2(0) NOT NULL CONSTRAINT DF_TKVT_NgayGan DEFAULT SYSUTCDATETIME(),
    NguoiGanTaiKhoanId int NULL,
    CONSTRAINT PK_TaiKhoanVaiTro PRIMARY KEY(TaiKhoanId, VaiTroId),
    CONSTRAINT FK_TKVT_TaiKhoan FOREIGN KEY(TaiKhoanId) REFERENCES dbo.TaiKhoan(TaiKhoanId),
    CONSTRAINT FK_TKVT_VaiTro FOREIGN KEY(VaiTroId) REFERENCES dbo.VaiTro(VaiTroId),
    CONSTRAINT FK_TKVT_NguoiGan FOREIGN KEY(NguoiGanTaiKhoanId) REFERENCES dbo.TaiKhoan(TaiKhoanId)
);
GO

/* Liên kết tài khoản với hồ sơ nghiệp vụ; mỗi hồ sơ chỉ gắn một tài khoản. */
IF OBJECT_ID('dbo.TaiKhoanSinhVien','U') IS NULL
CREATE TABLE dbo.TaiKhoanSinhVien (
    TaiKhoanId int NOT NULL CONSTRAINT PK_TaiKhoanSinhVien PRIMARY KEY,
    SinhVienId int NOT NULL,
    CONSTRAINT UQ_TaiKhoanSinhVien_SV UNIQUE(SinhVienId),
    CONSTRAINT FK_TKSV_TaiKhoan FOREIGN KEY(TaiKhoanId) REFERENCES dbo.TaiKhoan(TaiKhoanId),
    CONSTRAINT FK_TKSV_SinhVien FOREIGN KEY(SinhVienId) REFERENCES dbo.SinhVien(SinhVienId)
);
GO
IF OBJECT_ID('dbo.TaiKhoanGiangVien','U') IS NULL
CREATE TABLE dbo.TaiKhoanGiangVien (
    TaiKhoanId int NOT NULL CONSTRAINT PK_TaiKhoanGiangVien PRIMARY KEY,
    GiangVienId int NOT NULL,
    CONSTRAINT UQ_TaiKhoanGiangVien_GV UNIQUE(GiangVienId),
    CONSTRAINT FK_TKGV_TaiKhoan FOREIGN KEY(TaiKhoanId) REFERENCES dbo.TaiKhoan(TaiKhoanId),
    CONSTRAINT FK_TKGV_GiangVien FOREIGN KEY(GiangVienId) REFERENCES dbo.GiangVien(GiangVienId)
);
GO
IF OBJECT_ID('dbo.TaiKhoanNhanVienPGV','U') IS NULL
CREATE TABLE dbo.TaiKhoanNhanVienPGV (
    TaiKhoanId int NOT NULL CONSTRAINT PK_TaiKhoanNhanVienPGV PRIMARY KEY,
    NhanVienPGVId int NOT NULL,
    CONSTRAINT UQ_TaiKhoanNhanVienPGV_PGV UNIQUE(NhanVienPGVId),
    CONSTRAINT FK_TKPGV_TaiKhoan FOREIGN KEY(TaiKhoanId) REFERENCES dbo.TaiKhoan(TaiKhoanId),
    CONSTRAINT FK_TKPGV_NhanVien FOREIGN KEY(NhanVienPGVId) REFERENCES dbo.NhanVienPGV(NhanVienPGVId)
);
GO

/* Khởi tạo 4 vai trò hệ thống; không tạo tài khoản/mật khẩu mặc định. */
IF NOT EXISTS(SELECT 1 FROM dbo.VaiTro WHERE MaVaiTro='ADMIN')
    INSERT dbo.VaiTro(MaVaiTro,TenVaiTro,MoTa) VALUES('ADMIN',N'Admin',N'Quản trị tài khoản, phân quyền và cấu hình hệ thống');
IF NOT EXISTS(SELECT 1 FROM dbo.VaiTro WHERE MaVaiTro='PGV')
    INSERT dbo.VaiTro(MaVaiTro,TenVaiTro,MoTa) VALUES('PGV',N'Phòng giáo vụ',N'Quản lý hồ sơ đào tạo, học kỳ, lớp học phần, đăng ký và kết quả');
IF NOT EXISTS(SELECT 1 FROM dbo.VaiTro WHERE MaVaiTro='GV')
    INSERT dbo.VaiTro(MaVaiTro,TenVaiTro,MoTa) VALUES('GV',N'Giáo viên/Giảng viên',N'Xem lớp được phân công, điểm danh và nhập điểm theo quyền');
IF NOT EXISTS(SELECT 1 FROM dbo.VaiTro WHERE MaVaiTro='HS')
    INSERT dbo.VaiTro(MaVaiTro,TenVaiTro,MoTa) VALUES('HS',N'Học sinh/Sinh viên',N'Xem thông tin cá nhân, đăng ký học phần và xem kết quả học tập');
GO

CREATE OR ALTER VIEW dbo.vw_TaiKhoanVaiTro
AS
SELECT tk.TaiKhoanId, tk.TenDangNhap, tk.HoTenHienThi, tk.Email,
       tk.TrangThai AS TrangThaiTaiKhoan, vt.MaVaiTro, vt.TenVaiTro,
       sv.MaSinhVien, gv.MaGiangVien, pgv.MaNhanVien AS MaNhanVienPGV
FROM dbo.TaiKhoan tk
LEFT JOIN dbo.TaiKhoanVaiTro tkvt ON tkvt.TaiKhoanId=tk.TaiKhoanId
LEFT JOIN dbo.VaiTro vt ON vt.VaiTroId=tkvt.VaiTroId
LEFT JOIN dbo.TaiKhoanSinhVien tksv ON tksv.TaiKhoanId=tk.TaiKhoanId
LEFT JOIN dbo.SinhVien sv ON sv.SinhVienId=tksv.SinhVienId
LEFT JOIN dbo.TaiKhoanGiangVien tkgv ON tkgv.TaiKhoanId=tk.TaiKhoanId
LEFT JOIN dbo.GiangVien gv ON gv.GiangVienId=tkgv.GiangVienId
LEFT JOIN dbo.TaiKhoanNhanVienPGV tkpgv ON tkpgv.TaiKhoanId=tk.TaiKhoanId
LEFT JOIN dbo.NhanVienPGV pgv ON pgv.NhanVienPGVId=tkpgv.NhanVienPGVId;
GO

/*
   Ví dụ thao tác (chạy sau khi đã có dữ liệu tương ứng):
   -- Xem danh sách vai trò:
   SELECT * FROM dbo.VaiTro ORDER BY VaiTroId;
   -- Xem tài khoản và vai trò đã gán:
   SELECT * FROM dbo.vw_TaiKhoanVaiTro ORDER BY TenDangNhap;
   -- Đăng ký học lại: tạo một bản ghi DangKyHocPhan mới cho lớp học phần mới.
   -- Không chuyển SinhVien.LopId; sinh viên vẫn thuộc lớp hành chính ban đầu.
*/
GO
