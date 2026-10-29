using System;
using System.Collections.Generic;
using BTL_StudentMGMT.Models.ScaffoldedStable;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Data.ScaffoldedStable;

public partial class QLSinhVienUtcScaffoldContext : DbContext
{
    public QLSinhVienUtcScaffoldContext(DbContextOptions<QLSinhVienUtcScaffoldContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BieuPhiHocPhi> BieuPhiHocPhis { get; set; }

    public virtual DbSet<BuoiDiemDanh> BuoiDiemDanhs { get; set; }

    public virtual DbSet<CapHocBong> CapHocBongs { get; set; }

    public virtual DbSet<ChiTietHoaDonHocPhi> ChiTietHoaDonHocPhis { get; set; }

    public virtual DbSet<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; }

    public virtual DbSet<ChuongTrinhHocBong> ChuongTrinhHocBongs { get; set; }

    public virtual DbSet<ChuongTrinhMonHoc> ChuongTrinhMonHocs { get; set; }

    public virtual DbSet<DangKyHocPhan> DangKyHocPhans { get; set; }

    public virtual DbSet<DiemDanh> DiemDanhs { get; set; }

    public virtual DbSet<DiemThanhPhan> DiemThanhPhans { get; set; }

    public virtual DbSet<DonVi> DonVis { get; set; }

    public virtual DbSet<GiangVien> GiangViens { get; set; }

    public virtual DbSet<HoSoSinhVien> HoSoSinhViens { get; set; }

    public virtual DbSet<HoaDonHocPhi> HoaDonHocPhis { get; set; }

    public virtual DbSet<HocKy> HocKies { get; set; }

    public virtual DbSet<HocPhanTienQuyet> HocPhanTienQuyets { get; set; }

    public virtual DbSet<KetQuaHocTap> KetQuaHocTaps { get; set; }

    public virtual DbSet<KhoaTuyenSinh> KhoaTuyenSinhs { get; set; }

    public virtual DbSet<KyLuatSinhVien> KyLuatSinhViens { get; set; }

    public virtual DbSet<LichHoc> LichHocs { get; set; }

    public virtual DbSet<LichSuChinhSuaDiem> LichSuChinhSuaDiems { get; set; }

    public virtual DbSet<LichSuTrangThaiSinhVien> LichSuTrangThaiSinhViens { get; set; }

    public virtual DbSet<LopHanhChinh> LopHanhChinhs { get; set; }

    public virtual DbSet<LopHocPhan> LopHocPhans { get; set; }

    public virtual DbSet<MonHoc> MonHocs { get; set; }

    public virtual DbSet<NamHoc> NamHocs { get; set; }

    public virtual DbSet<Nganh> Nganhs { get; set; }

    public virtual DbSet<NhanVienPGV> NhanVienPGVs { get; set; }

    public virtual DbSet<NhatKyKiemToan> NhatKyKiemToans { get; set; }

    public virtual DbSet<PhongHoc> PhongHocs { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TaiKhoanGiangVien> TaiKhoanGiangViens { get; set; }

    public virtual DbSet<TaiKhoanNhanVienPGV> TaiKhoanNhanVienPGVs { get; set; }

    public virtual DbSet<TaiKhoanSinhVien> TaiKhoanSinhViens { get; set; }

    public virtual DbSet<TaiKhoanVaiTro> TaiKhoanVaiTros { get; set; }

    public virtual DbSet<ThanhPhanDiem> ThanhPhanDiems { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<vw_CongNoHocPhi> vw_CongNoHocPhis { get; set; }

    public virtual DbSet<vw_DangKyHocPhanChiTiet> vw_DangKyHocPhanChiTiets { get; set; }

    public virtual DbSet<vw_DanhBaSinhVien> vw_DanhBaSinhViens { get; set; }

    public virtual DbSet<vw_GPAHocKy> vw_GPAHocKies { get; set; }

    public virtual DbSet<vw_TaiKhoanVaiTro> vw_TaiKhoanVaiTros { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BieuPhiHocPhi>(entity =>
        {
            entity.HasKey(e => e.BieuPhiId);

            entity.ToTable("BieuPhiHocPhi");

            entity.Property(e => e.DonGiaTinChi).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LoaiTien)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValue("VND", "DF_BieuPhi_Tien");

            entity.HasOne(d => d.ChuongTrinh).WithMany(p => p.BieuPhiHocPhis)
                .HasForeignKey(d => d.ChuongTrinhId)
                .HasConstraintName("FK_BieuPhi_CTD");

            entity.HasOne(d => d.NamHoc).WithMany(p => p.BieuPhiHocPhis)
                .HasForeignKey(d => d.NamHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BieuPhi_NamHoc");
        });

        modelBuilder.Entity<BuoiDiemDanh>(entity =>
        {
            entity.ToTable("BuoiDiemDanh");

            entity.Property(e => e.NguoiTao).HasMaxLength(450);
            entity.Property(e => e.NoiDung).HasMaxLength(200);
            entity.Property(e => e.ThoiGianBatDau).HasPrecision(0);
            entity.Property(e => e.ThoiGianKetThuc).HasPrecision(0);

            entity.HasOne(d => d.LopHocPhan).WithMany(p => p.BuoiDiemDanhs)
                .HasForeignKey(d => d.LopHocPhanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BDD_LHP");
        });

        modelBuilder.Entity<CapHocBong>(entity =>
        {
            entity.ToTable("CapHocBong");

            entity.HasIndex(e => new { e.HocBongId, e.SinhVienId }, "UQ_CapHB_HB_SV").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.SoQuyetDinh)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("APPROVED", "DF_CapHocBong_TrangThai");

            entity.HasOne(d => d.HocBong).WithMany(p => p.CapHocBongs)
                .HasForeignKey(d => d.HocBongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CapHB_HB");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.CapHocBongs)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CapHB_SV");
        });

        modelBuilder.Entity<ChiTietHoaDonHocPhi>(entity =>
        {
            entity.HasKey(e => e.ChiTietHoaDonId).HasName("PK_ChiTietHoaDon");

            entity.ToTable("ChiTietHoaDonHocPhi");

            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LoaiKhoanThu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(300);
            entity.Property(e => e.SoLuong)
                .HasDefaultValue(1m, "DF_CTHD_SL")
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SoTienGiam).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.HoaDon).WithMany(p => p.ChiTietHoaDonHocPhis)
                .HasForeignKey(d => d.HoaDonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTHD_HD");
        });

        modelBuilder.Entity<ChuongTrinhDaoTao>(entity =>
        {
            entity.HasKey(e => e.ChuongTrinhId);

            entity.ToTable("ChuongTrinhDaoTao");

            entity.HasIndex(e => e.MaChuongTrinh, "UQ_CTD_Ma").IsUnique();

            entity.Property(e => e.MaChuongTrinh)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SoNamDaoTao).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.TenChuongTrinh).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_CTD_TrangThai");
            entity.Property(e => e.TrinhDo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("DAI_HOC", "DF_CTD_TrinhDo");

            entity.HasOne(d => d.Nganh).WithMany(p => p.ChuongTrinhDaoTaos)
                .HasForeignKey(d => d.NganhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTD_Nganh");
        });

        modelBuilder.Entity<ChuongTrinhHocBong>(entity =>
        {
            entity.HasKey(e => e.HocBongId);

            entity.ToTable("ChuongTrinhHocBong");

            entity.HasIndex(e => e.MaHocBong, "UQ_HocBong_Ma").IsUnique();

            entity.Property(e => e.MaHocBong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SoTienToiDa).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenHocBong).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("OPEN", "DF_HocBong_TrangThai");

            entity.HasOne(d => d.NamHoc).WithMany(p => p.ChuongTrinhHocBongs)
                .HasForeignKey(d => d.NamHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocBong_NamHoc");
        });

        modelBuilder.Entity<ChuongTrinhMonHoc>(entity =>
        {
            entity.HasKey(e => new { e.ChuongTrinhId, e.MonHocId });

            entity.ToTable("ChuongTrinhMonHoc");

            entity.Property(e => e.BatBuoc).HasDefaultValue(true, "DF_CTMH_BatBuoc");
            entity.Property(e => e.GhiChu).HasMaxLength(300);

            entity.HasOne(d => d.ChuongTrinh).WithMany(p => p.ChuongTrinhMonHocs)
                .HasForeignKey(d => d.ChuongTrinhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTMH_CTD");

            entity.HasOne(d => d.MonHoc).WithMany(p => p.ChuongTrinhMonHocs)
                .HasForeignKey(d => d.MonHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTMH_Mon");
        });

        modelBuilder.Entity<DangKyHocPhan>(entity =>
        {
            entity.HasKey(e => e.DangKyId);

            entity.ToTable("DangKyHocPhan");

            entity.HasIndex(e => new { e.LopHocPhanId, e.TrangThai }, "IX_DK_LHP_TrangThai");

            entity.HasIndex(e => new { e.SinhVienId, e.TrangThai }, "IX_DK_SV_TrangThai");

            entity.HasIndex(e => new { e.SinhVienId, e.LopHocPhanId }, "UQ_DK_SV_LHP").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(300);
            entity.Property(e => e.LanHoc).HasDefaultValue((byte)1, "DF_DK_LanHoc");
            entity.Property(e => e.NgayDangKy)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_DK_Ngay");
            entity.Property(e => e.RowVer)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ENROLLED", "DF_DK_TrangThai");

            entity.HasOne(d => d.LopHocPhan).WithMany(p => p.DangKyHocPhans)
                .HasForeignKey(d => d.LopHocPhanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DK_LHP");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.DangKyHocPhans)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DK_SV");
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.HasKey(e => new { e.BuoiDiemDanhId, e.SinhVienId });

            entity.ToTable("DiemDanh");

            entity.Property(e => e.CapNhatLuc)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_DiemDanh_Time");
            entity.Property(e => e.GhiChu).HasMaxLength(300);
            entity.Property(e => e.NguoiCapNhat).HasMaxLength(450);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.BuoiDiemDanh).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.BuoiDiemDanhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_Buoi");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.DiemDanhs)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_SV");
        });

        modelBuilder.Entity<DiemThanhPhan>(entity =>
        {
            entity.HasKey(e => new { e.DangKyId, e.ThanhPhanDiemId });

            entity.ToTable("DiemThanhPhan");

            entity.Property(e => e.Diem).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.NgayCapNhat).HasPrecision(0);
            entity.Property(e => e.NgayNhap).HasPrecision(0);
            entity.Property(e => e.NguoiNhap).HasMaxLength(450);
            entity.Property(e => e.NhanXet).HasMaxLength(500);

            entity.HasOne(d => d.DangKy).WithMany(p => p.DiemThanhPhans)
                .HasForeignKey(d => d.DangKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DTP_DK");

            entity.HasOne(d => d.ThanhPhanDiem).WithMany(p => p.DiemThanhPhans)
                .HasForeignKey(d => d.ThanhPhanDiemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DTP_TPD");
        });

        modelBuilder.Entity<DonVi>(entity =>
        {
            entity.ToTable("DonVi");

            entity.HasIndex(e => e.MaDonVi, "UQ_DonVi_Ma").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_DonVi_CreatedAt");
            entity.Property(e => e.DienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.LoaiDonVi)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaDonVi)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RowVer)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TenDonVi).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_DonVi_TrangThai");
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.DonViCha).WithMany(p => p.InverseDonViCha)
                .HasForeignKey(d => d.DonViChaId)
                .HasConstraintName("FK_DonVi_Cha");
        });

        modelBuilder.Entity<GiangVien>(entity =>
        {
            entity.ToTable("GiangVien");

            entity.HasIndex(e => e.IdentityUserId, "UQ_GiangVien_IdentityUser").IsUnique();

            entity.HasIndex(e => e.MaGiangVien, "UQ_GiangVien_Ma").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.Ho).HasMaxLength(80);
            entity.Property(e => e.MaGiangVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Ten).HasMaxLength(50);
            entity.Property(e => e.TenDem).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_GiangVien_TrangThai");

            entity.HasOne(d => d.DonVi).WithMany(p => p.GiangViens)
                .HasForeignKey(d => d.DonViId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GiangVien_DonVi");
        });

        modelBuilder.Entity<HoSoSinhVien>(entity =>
        {
            entity.HasKey(e => e.HoSoId);

            entity.ToTable("HoSoSinhVien");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_HoSo_Created");
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.LoaiHoSo)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NgayDuyet).HasPrecision(0);
            entity.Property(e => e.NguoiDuyet).HasMaxLength(450);
            entity.Property(e => e.TenTep).HasMaxLength(255);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDING", "DF_HoSo_TrangThai");
            entity.Property(e => e.UriLuuTru).HasMaxLength(1000);

            entity.HasOne(d => d.SinhVien).WithMany(p => p.HoSoSinhViens)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoSo_SV");
        });

        modelBuilder.Entity<HoaDonHocPhi>(entity =>
        {
            entity.HasKey(e => e.HoaDonId);

            entity.ToTable("HoaDonHocPhi");

            entity.HasIndex(e => e.SoHoaDon, "UQ_HoaDon_So").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.NgayLap)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_HoaDon_Ngay");
            entity.Property(e => e.SoHoaDon)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ISSUED", "DF_HoaDon_TrangThai");

            entity.HasOne(d => d.HocKy).WithMany(p => p.HoaDonHocPhis)
                .HasForeignKey(d => d.HocKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoaDon_HocKy");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.HoaDonHocPhis)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HoaDon_SV");
        });

        modelBuilder.Entity<HocKy>(entity =>
        {
            entity.ToTable("HocKy");

            entity.HasIndex(e => new { e.NamHocId, e.MaHocKy }, "UQ_HocKy").IsUnique();

            entity.Property(e => e.MaHocKy)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenHocKy).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PLANNED", "DF_HocKy_TrangThai");

            entity.HasOne(d => d.NamHoc).WithMany(p => p.HocKies)
                .HasForeignKey(d => d.NamHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocKy_NamHoc");
        });

        modelBuilder.Entity<HocPhanTienQuyet>(entity =>
        {
            entity.HasKey(e => new { e.MonHocId, e.MonTienQuyetId });

            entity.ToTable("HocPhanTienQuyet");

            entity.Property(e => e.LoaiDieuKien)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PASSED", "DF_TienQuyet_Loai");

            entity.HasOne(d => d.MonHoc).WithMany(p => p.HocPhanTienQuyetMonHocs)
                .HasForeignKey(d => d.MonHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TienQuyet_Mon");

            entity.HasOne(d => d.MonTienQuyet).WithMany(p => p.HocPhanTienQuyetMonTienQuyets)
                .HasForeignKey(d => d.MonTienQuyetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TienQuyet_TienQuyet");
        });

        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.HasKey(e => e.DangKyId);

            entity.ToTable("KetQuaHocTap");

            entity.Property(e => e.DangKyId).ValueGeneratedNever();
            entity.Property(e => e.DiemChu)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.DiemHe4).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.DiemTongKet).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.NgayChot).HasPrecision(0);
            entity.Property(e => e.NguoiChot).HasMaxLength(450);
            entity.Property(e => e.PhienBan).HasDefaultValue(1, "DF_KQ_PhienBan");

            entity.HasOne(d => d.DangKy).WithOne(p => p.KetQuaHocTap)
                .HasForeignKey<KetQuaHocTap>(d => d.DangKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KQ_DK");
        });

        modelBuilder.Entity<KhoaTuyenSinh>(entity =>
        {
            entity.ToTable("KhoaTuyenSinh");

            entity.HasIndex(e => e.MaKhoa, "UQ_KhoaTuyenSinh_Ma").IsUnique();

            entity.Property(e => e.MaKhoa)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenKhoa).HasMaxLength(100);
        });

        modelBuilder.Entity<KyLuatSinhVien>(entity =>
        {
            entity.HasKey(e => e.KyLuatId);

            entity.ToTable("KyLuatSinhVien");

            entity.Property(e => e.LoaiViPham)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(1000);
            entity.Property(e => e.NguoiLap).HasMaxLength(450);
            entity.Property(e => e.SoQuyetDinh)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_KyLuat_TrangThai");

            entity.HasOne(d => d.SinhVien).WithMany(p => p.KyLuatSinhViens)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KyLuat_SV");
        });

        modelBuilder.Entity<LichHoc>(entity =>
        {
            entity.ToTable("LichHoc");

            entity.HasIndex(e => e.LopHocPhanId, "IX_LichHoc_LHP");

            entity.Property(e => e.GioBatDau).HasPrecision(0);
            entity.Property(e => e.GioKetThuc).HasPrecision(0);

            entity.HasOne(d => d.LopHocPhan).WithMany(p => p.LichHocs)
                .HasForeignKey(d => d.LopHocPhanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichHoc_LHP");

            entity.HasOne(d => d.PhongHoc).WithMany(p => p.LichHocs)
                .HasForeignKey(d => d.PhongHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichHoc_Phong");
        });

        modelBuilder.Entity<LichSuChinhSuaDiem>(entity =>
        {
            entity.HasKey(e => e.LichSuDiemId);

            entity.ToTable("LichSuChinhSuaDiem");

            entity.Property(e => e.DiemCu).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.DiemMoi).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.LyDo).HasMaxLength(500);
            entity.Property(e => e.NguoiThayDoi).HasMaxLength(450);
            entity.Property(e => e.ThoiDiem)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_LS_Diem_Time");

            entity.HasOne(d => d.DangKy).WithMany(p => p.LichSuChinhSuaDiems)
                .HasForeignKey(d => d.DangKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LS_Diem_DK");
        });

        modelBuilder.Entity<LichSuTrangThaiSinhVien>(entity =>
        {
            entity.HasKey(e => e.LichSuId);

            entity.ToTable("LichSuTrangThaiSinhVien");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_LichSuSV_Created");
            entity.Property(e => e.LyDo).HasMaxLength(500);
            entity.Property(e => e.NguoiThayDoi).HasMaxLength(450);
            entity.Property(e => e.TrangThaiCu)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.TrangThaiMoi)
                .HasMaxLength(25)
                .IsUnicode(false);

            entity.HasOne(d => d.SinhVien).WithMany(p => p.LichSuTrangThaiSinhViens)
                .HasForeignKey(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichSuSV_SV");
        });

        modelBuilder.Entity<LopHanhChinh>(entity =>
        {
            entity.HasKey(e => e.LopId);

            entity.ToTable("LopHanhChinh");

            entity.HasIndex(e => e.MaLop, "UQ_Lop_Ma").IsUnique();

            entity.Property(e => e.MaLop)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TenLop).HasMaxLength(150);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_Lop_TrangThai");

            entity.HasOne(d => d.ChuongTrinh).WithMany(p => p.LopHanhChinhs)
                .HasForeignKey(d => d.ChuongTrinhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lop_CTD");

            entity.HasOne(d => d.CoVanGiangVien).WithMany(p => p.LopHanhChinhs)
                .HasForeignKey(d => d.CoVanGiangVienId)
                .HasConstraintName("FK_Lop_CoVan");

            entity.HasOne(d => d.KhoaTuyenSinh).WithMany(p => p.LopHanhChinhs)
                .HasForeignKey(d => d.KhoaTuyenSinhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lop_Khoa");

            entity.HasOne(d => d.Nganh).WithMany(p => p.LopHanhChinhs)
                .HasForeignKey(d => d.NganhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lop_Nganh");
        });

        modelBuilder.Entity<LopHocPhan>(entity =>
        {
            entity.ToTable("LopHocPhan");

            entity.HasIndex(e => new { e.HocKyId, e.MonHocId, e.TrangThai }, "IX_LHP_HocKy_Mon");

            entity.HasIndex(e => e.MaLopHocPhan, "UQ_LHP_Ma").IsUnique();

            entity.Property(e => e.MaLopHocPhan)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Nhom)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("01", "DF_LHP_Nhom");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("PLANNED", "DF_LHP_TrangThai");

            entity.HasOne(d => d.GiangVien).WithMany(p => p.LopHocPhans)
                .HasForeignKey(d => d.GiangVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LHP_GV");

            entity.HasOne(d => d.HocKy).WithMany(p => p.LopHocPhans)
                .HasForeignKey(d => d.HocKyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LHP_HocKy");

            entity.HasOne(d => d.MonHoc).WithMany(p => p.LopHocPhans)
                .HasForeignKey(d => d.MonHocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LHP_Mon");
        });

        modelBuilder.Entity<MonHoc>(entity =>
        {
            entity.ToTable("MonHoc");

            entity.HasIndex(e => e.MaMon, "UQ_MonHoc_Ma").IsUnique();

            entity.Property(e => e.LoaiMon)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("BAT_BUOC", "DF_MonHoc_Loai");
            entity.Property(e => e.MaMon)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TenMon).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_MonHoc_TrangThai");

            entity.HasOne(d => d.DonViPhuTrach).WithMany(p => p.MonHocs)
                .HasForeignKey(d => d.DonViPhuTrachId)
                .HasConstraintName("FK_MonHoc_DonVi");
        });

        modelBuilder.Entity<NamHoc>(entity =>
        {
            entity.ToTable("NamHoc");

            entity.HasIndex(e => e.MaNamHoc, "UQ_NamHoc_Ma").IsUnique();

            entity.Property(e => e.MaNamHoc)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PLANNED", "DF_NamHoc_TrangThai");
        });

        modelBuilder.Entity<Nganh>(entity =>
        {
            entity.ToTable("Nganh");

            entity.HasIndex(e => e.MaNganh, "UQ_Nganh_Ma").IsUnique();

            entity.Property(e => e.MaNganh)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenNganh).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_Nganh_TrangThai");

            entity.HasOne(d => d.DonVi).WithMany(p => p.Nganhs)
                .HasForeignKey(d => d.DonViId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Nganh_DonVi");
        });

        modelBuilder.Entity<NhanVienPGV>(entity =>
        {
            entity.ToTable("NhanVienPGV");

            entity.HasIndex(e => e.MaNhanVien, "UQ_NhanVienPGV_Ma").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_NhanVienPGV_CreatedAt");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_NhanVienPGV_TrangThai");

            entity.HasOne(d => d.DonVi).WithMany(p => p.NhanVienPGVs)
                .HasForeignKey(d => d.DonViId)
                .HasConstraintName("FK_NhanVienPGV_DonVi");
        });

        modelBuilder.Entity<NhatKyKiemToan>(entity =>
        {
            entity.HasKey(e => e.NhatKyId);

            entity.ToTable("NhatKyKiemToan");

            entity.HasIndex(e => new { e.TenBang, e.ThoiDiem }, "IX_Audit_Bang_ThoiDiem").IsDescending(false, true);

            entity.Property(e => e.CorrelationId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HanhDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.KhoaBanGhi).HasMaxLength(200);
            entity.Property(e => e.TenBang)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.ThoiDiem)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Audit_Time");
            entity.Property(e => e.UserId).HasMaxLength(450);
        });

        modelBuilder.Entity<PhongHoc>(entity =>
        {
            entity.ToTable("PhongHoc");

            entity.HasIndex(e => e.MaPhong, "UQ_PhongHoc_Ma").IsUnique();

            entity.Property(e => e.CoSo).HasMaxLength(100);
            entity.Property(e => e.LoaiPhong)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("LY_THUYET", "DF_Phong_Loai");
            entity.Property(e => e.MaPhong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TenPhong).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_Phong_TrangThai");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.ToTable("SinhVien");

            entity.HasIndex(e => new { e.Ho, e.Ten }, "IX_SinhVien_HoTen");

            entity.HasIndex(e => new { e.LopId, e.TrangThai }, "IX_SinhVien_Lop_TrangThai");

            entity.HasIndex(e => e.IdentityUserId, "UQ_SinhVien_IdentityUser").IsUnique();

            entity.HasIndex(e => e.MaSinhVien, "UQ_SinhVien_Ma").IsUnique();

            entity.Property(e => e.CCCDHash).HasMaxLength(64);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_SinhVien_CreatedAt");
            entity.Property(e => e.DiaChiLienHe).HasMaxLength(500);
            entity.Property(e => e.DiaChiThuongTru).HasMaxLength(500);
            entity.Property(e => e.EmailCaNhan)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.EmailTruong)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Ho).HasMaxLength(80);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RowVer)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Ten).HasMaxLength(50);
            entity.Property(e => e.TenDem).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("STUDYING", "DF_SinhVien_TrangThai");
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Lop).WithMany(p => p.SinhViens)
                .HasForeignKey(d => d.LopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SinhVien_Lop");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ_TaiKhoan_TenDangNhap").IsUnique();

            entity.HasIndex(e => e.Email, "UX_TaiKhoan_Email")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.BuocDoiMatKhau).HasDefaultValue(true, "DF_TaiKhoan_BuocDoiMK");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_TaiKhoan_CreatedAt");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.HoTenHienThi).HasMaxLength(150);
            entity.Property(e => e.KhoaDen).HasPrecision(0);
            entity.Property(e => e.LanDangNhapCuoi).HasPrecision(0);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.RowVer)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("ACTIVE", "DF_TaiKhoan_TrangThai");
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
        });

        modelBuilder.Entity<TaiKhoanGiangVien>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId);

            entity.ToTable("TaiKhoanGiangVien");

            entity.HasIndex(e => e.GiangVienId, "UQ_TaiKhoanGiangVien_GV").IsUnique();

            entity.Property(e => e.TaiKhoanId).ValueGeneratedNever();

            entity.HasOne(d => d.GiangVien).WithOne(p => p.TaiKhoanGiangVien)
                .HasForeignKey<TaiKhoanGiangVien>(d => d.GiangVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKGV_GiangVien");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.TaiKhoanGiangVien)
                .HasForeignKey<TaiKhoanGiangVien>(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKGV_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoanNhanVienPGV>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId);

            entity.ToTable("TaiKhoanNhanVienPGV");

            entity.HasIndex(e => e.NhanVienPGVId, "UQ_TaiKhoanNhanVienPGV_PGV").IsUnique();

            entity.Property(e => e.TaiKhoanId).ValueGeneratedNever();

            entity.HasOne(d => d.NhanVienPGV).WithOne(p => p.TaiKhoanNhanVienPGV)
                .HasForeignKey<TaiKhoanNhanVienPGV>(d => d.NhanVienPGVId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKPGV_NhanVien");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.TaiKhoanNhanVienPGV)
                .HasForeignKey<TaiKhoanNhanVienPGV>(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKPGV_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoanSinhVien>(entity =>
        {
            entity.HasKey(e => e.TaiKhoanId);

            entity.ToTable("TaiKhoanSinhVien");

            entity.HasIndex(e => e.SinhVienId, "UQ_TaiKhoanSinhVien_SV").IsUnique();

            entity.Property(e => e.TaiKhoanId).ValueGeneratedNever();

            entity.HasOne(d => d.SinhVien).WithOne(p => p.TaiKhoanSinhVien)
                .HasForeignKey<TaiKhoanSinhVien>(d => d.SinhVienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKSV_SinhVien");

            entity.HasOne(d => d.TaiKhoan).WithOne(p => p.TaiKhoanSinhVien)
                .HasForeignKey<TaiKhoanSinhVien>(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKSV_TaiKhoan");
        });

        modelBuilder.Entity<TaiKhoanVaiTro>(entity =>
        {
            entity.HasKey(e => new { e.TaiKhoanId, e.VaiTroId });

            entity.ToTable("TaiKhoanVaiTro");

            entity.Property(e => e.NgayGan)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_TKVT_NgayGan");

            entity.HasOne(d => d.NguoiGanTaiKhoan).WithMany(p => p.TaiKhoanVaiTroNguoiGanTaiKhoans)
                .HasForeignKey(d => d.NguoiGanTaiKhoanId)
                .HasConstraintName("FK_TKVT_NguoiGan");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.TaiKhoanVaiTroTaiKhoans)
                .HasForeignKey(d => d.TaiKhoanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKVT_TaiKhoan");

            entity.HasOne(d => d.VaiTro).WithMany(p => p.TaiKhoanVaiTros)
                .HasForeignKey(d => d.VaiTroId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TKVT_VaiTro");
        });

        modelBuilder.Entity<ThanhPhanDiem>(entity =>
        {
            entity.ToTable("ThanhPhanDiem");

            entity.HasIndex(e => new { e.LopHocPhanId, e.MaThanhPhan }, "UQ_TPD_LHP_Ma").IsUnique();

            entity.Property(e => e.BatBuoc).HasDefaultValue(true, "DF_TPD_BatBuoc");
            entity.Property(e => e.DiemToiDa)
                .HasDefaultValue(10m, "DF_TPD_DiemMax")
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MaThanhPhan)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TenThanhPhan).HasMaxLength(100);
            entity.Property(e => e.TrongSo).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.LopHocPhan).WithMany(p => p.ThanhPhanDiems)
                .HasForeignKey(d => d.LopHocPhanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TPD_LHP");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.ToTable("ThanhToan");

            entity.HasIndex(e => new { e.HoaDonId, e.TrangThai }, "IX_ThanhToan_HD_TrangThai");

            entity.HasIndex(e => e.IdempotencyKey, "UQ_ThanhToan_Idempotency").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MaGiaoDich)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NguonThanhToan).HasMaxLength(100);
            entity.Property(e => e.PhuongThuc)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ThoiGianThanhToan)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_ThanhToan_Time");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDING", "DF_ThanhToan_TrangThai");

            entity.HasOne(d => d.HoaDon).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.HoaDonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhToan_HD");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.MaVaiTro, "UQ_VaiTro_Ma").IsUnique();

            entity.Property(e => e.VaiTroId).ValueGeneratedOnAdd();
            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(300);
            entity.Property(e => e.TenVaiTro).HasMaxLength(100);
        });

        modelBuilder.Entity<vw_CongNoHocPhi>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CongNoHocPhi");

            entity.Property(e => e.ConNo).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.DaThanhToan).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.HoTen).HasMaxLength(232);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoHoaDon)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.TenHocKy).HasMaxLength(100);
            entity.Property(e => e.TongTien).HasColumnType("decimal(38, 4)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<vw_DangKyHocPhanChiTiet>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_DangKyHocPhanChiTiet");

            entity.Property(e => e.DiemChu)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.DiemHe4).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.DiemTongKet).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.HoTen).HasMaxLength(232);
            entity.Property(e => e.MaLopHocPhan)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaMon)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaNamHoc)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenHocKy).HasMaxLength(100);
            entity.Property(e => e.TenMon).HasMaxLength(200);
            entity.Property(e => e.TrangThaiDangKy)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<vw_DanhBaSinhVien>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_DanhBaSinhVien");

            entity.Property(e => e.EmailTruong)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(232);
            entity.Property(e => e.MaLop)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaNganh)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenChuongTrinh).HasMaxLength(200);
            entity.Property(e => e.TenLop).HasMaxLength(150);
            entity.Property(e => e.TenNganh).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(25)
                .IsUnicode(false);
        });

        modelBuilder.Entity<vw_GPAHocKy>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_GPAHocKy");

            entity.Property(e => e.DiemTBHe10).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.GPAHe4).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenHocKy).HasMaxLength(100);
        });

        modelBuilder.Entity<vw_TaiKhoanVaiTro>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_TaiKhoanVaiTro");

            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .IsUnicode(false);
            entity.Property(e => e.HoTenHienThi).HasMaxLength(150);
            entity.Property(e => e.MaGiangVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVienPGV)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaVaiTro)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TenVaiTro).HasMaxLength(100);
            entity.Property(e => e.TrangThaiTaiKhoan)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
