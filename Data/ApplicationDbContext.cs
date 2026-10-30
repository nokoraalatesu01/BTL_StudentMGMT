using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Data;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Nganh> Nganhs => Set<Nganh>();
    public DbSet<Lop> Lops => Set<Lop>();
    public DbSet<SinhVien> SinhViens => Set<SinhVien>();
    public DbSet<GiangVien> GiangViens => Set<GiangVien>();
    public DbSet<MonHoc> MonHocs => Set<MonHoc>();
    public DbSet<Diem> Diems => Set<Diem>();
    public DbSet<ChuongTrinhDaoTao> ChuongTrinhDaoTaos => Set<ChuongTrinhDaoTao>();
    public DbSet<DiemDanh> DiemDanhs => Set<DiemDanh>();
    public DbSet<LichGiangDay> LichGiangDays => Set<LichGiangDay>();

    // Schema mới (QLSinhVienUTC)
    public DbSet<DonVi> DonVis => Set<DonVi>();
    public DbSet<NamHoc> NamHocs => Set<NamHoc>();
    public DbSet<HocKy> HocKies => Set<HocKy>();
    public DbSet<KhoaTuyenSinh> KhoaTuyenSinhs => Set<KhoaTuyenSinh>();
    public DbSet<PhongHoc> PhongHocs => Set<PhongHoc>();
    // public DbSet<LopHanhChinh> LopHanhChinhs => Set<LopHanhChinh>();
    // public DbSet<LopHocPhan> LopHocPhans => Set<LopHocPhan>();
    public DbSet<LichHoc> LichHocs => Set<LichHoc>();
    // public DbSet<DangKyHocPhan> DangKyHocPhans => Set<DangKyHocPhan>();
    public DbSet<ThanhPhanDiem> ThanhPhanDiems => Set<ThanhPhanDiem>();
    public DbSet<DiemThanhPhan> DiemThanhPhans => Set<DiemThanhPhan>();
    public DbSet<KetQuaHocTap> KetQuaHocTaps => Set<KetQuaHocTap>();
    public DbSet<ChuongTrinhHocBong> ChuongTrinhHocBongs => Set<ChuongTrinhHocBong>();
    // Schema mở rộng (chưa kích hoạt nghiệp vụ):
    // public DbSet<BuoiDiemDanh> BuoiDiemDanhs => Set<BuoiDiemDanh>();
    // public DbSet<ChiTietHoaDonHocPhi> ChiTietHoaDonHocPhis => Set<ChiTietHoaDonHocPhi>();
    // public DbSet<HoaDonHocPhi> HoaDonHocPhis => Set<HoaDonHocPhi>();
    // public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    // public DbSet<BieuPhiHocPhi> BieuPhiHocPhis => Set<BieuPhiHocPhi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Nganh>()
            .Property(nganh => nganh.MaNganh)
            .HasMaxLength(20);
        modelBuilder.Entity<Nganh>()
            .Property(nganh => nganh.TenNganh)
            .HasMaxLength(150);
        modelBuilder.Entity<Nganh>()
            .Property(nganh => nganh.TenKhoa)
            .HasMaxLength(150);
        modelBuilder.Entity<Nganh>()
            .HasOne(nganh => nganh.DonVi)
            .WithMany(donVi => donVi.Nganhs)
            .HasForeignKey(nganh => nganh.DonViId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Nganh>()
            .Property(nganh => nganh.TichHopCuNhanKySu)
            .HasDefaultValue(false);
        modelBuilder.Entity<Nganh>()
            .HasIndex(nganh => nganh.MaNganh)
            .IsUnique();

        modelBuilder.Entity<Lop>()
            .Property(lop => lop.TenLop)
            .HasMaxLength(60);
        modelBuilder.Entity<Lop>()
            .HasIndex(lop => new { lop.NganhId, lop.TenLop })
            .IsUnique();
        modelBuilder.Entity<Lop>()
            .HasOne(lop => lop.Nganh)
            .WithMany(nganh => nganh.Lops)
            .HasForeignKey(lop => lop.NganhId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Lop>()
            .HasOne(lop => lop.ChuongTrinhDaoTao)
            .WithMany()
            .HasForeignKey(lop => lop.ChuongTrinhDaoTaoId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SinhVien>()
            .HasOne(sinhVien => sinhVien.Lop)
            .WithMany(lop => lop.SinhViens)
            .HasForeignKey(sinhVien => sinhVien.LopId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChuongTrinhHocBong>(entity =>
        {
            entity.HasKey(e => e.HocBongId);
            entity.ToTable("ChuongTrinhHocBong");
            entity.HasIndex(e => e.MaHocBong, "UQ_HocBong_Ma").IsUnique();
            entity.Property(e => e.MaHocBong).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.SoTienToiDa).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenHocBong).HasMaxLength(200);
            entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("OPEN", "DF_HocBong_TrangThai");
            entity.HasOne(d => d.NamHoc).WithMany(p => p.ChuongTrinhHocBongs).HasForeignKey(d => d.NamHocId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_HocBong_NamHoc");
        });
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .Property(chuongTrinh => chuongTrinh.TenChuongTrinh)
            .HasMaxLength(200);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .Property(chuongTrinh => chuongTrinh.LoaiChuongTrinh)
            .HasMaxLength(60);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .Property(chuongTrinh => chuongTrinh.KhoaQuanLy)
            .HasMaxLength(150);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .Property(chuongTrinh => chuongTrinh.KhoaChuyenMon)
            .HasMaxLength(150);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .Property(chuongTrinh => chuongTrinh.MaNganhGoc)
            .HasMaxLength(20);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .Property(chuongTrinh => chuongTrinh.HinhThucLienKet)
            .HasMaxLength(100);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .HasIndex(chuongTrinh => chuongTrinh.MaChuongTrinh)
            .IsUnique()
            .HasFilter("[MaChuongTrinh] IS NOT NULL");
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .HasOne(chuongTrinh => chuongTrinh.DonViQuanLy)
            .WithMany(donVi => donVi.ChuongTrinhQuanLy)
            .HasForeignKey(chuongTrinh => chuongTrinh.DonViQuanLyId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .HasOne(chuongTrinh => chuongTrinh.DonViChuyenMon)
            .WithMany(donVi => donVi.ChuongTrinhChuyenMon)
            .HasForeignKey(chuongTrinh => chuongTrinh.DonViChuyenMonId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .HasOne(chuongTrinh => chuongTrinh.Nganh)
            .WithMany(nganh => nganh.ChuongTrinhs)
            .HasForeignKey(chuongTrinh => chuongTrinh.NganhId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GiangVien>()
            .Property(giangVien => giangVien.MaGiangVien)
            .HasMaxLength(30);
        modelBuilder.Entity<GiangVien>()
            .Property(giangVien => giangVien.HoTen)
            .HasMaxLength(150);
        modelBuilder.Entity<GiangVien>()
            .Property(giangVien => giangVien.Email)
            .HasMaxLength(256);
        modelBuilder.Entity<GiangVien>()
            .Property(giangVien => giangVien.SoDienThoai)
            .HasMaxLength(30);
        modelBuilder.Entity<GiangVien>()
            .Property(giangVien => giangVien.HinhAnh)
            .HasMaxLength(500);
        modelBuilder.Entity<GiangVien>()
            .HasIndex(giangVien => giangVien.MaGiangVien)
            .IsUnique();
        modelBuilder.Entity<GiangVien>()
            .HasIndex(giangVien => giangVien.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");
        modelBuilder.Entity<GiangVien>()
            .HasOne(giangVien => giangVien.DonVi)
            .WithMany(donVi => donVi.GiangViens)
            .HasForeignKey(giangVien => giangVien.DonViId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SinhVien>()
            .Property(sinhVien => sinhVien.NienKhoa)
            .HasMaxLength(50);
        modelBuilder.Entity<SinhVien>()
            .Property(sinhVien => sinhVien.TrangThai)
            .HasMaxLength(50);
        modelBuilder.Entity<SinhVien>()
            .Property(sinhVien => sinhVien.HinhAnh)
            .HasMaxLength(500);
        modelBuilder.Entity<SinhVien>()
            .HasIndex(sinhVien => sinhVien.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        modelBuilder.Entity<MonHoc>()
            .Property(monHoc => monHoc.MaMon)
            .HasMaxLength(50);
        modelBuilder.Entity<MonHoc>()
            .HasIndex(monHoc => monHoc.MaMon)
            .IsUnique();
        modelBuilder.Entity<MonHoc>()
            .Property(monHoc => monHoc.TenMon)
            .HasMaxLength(255);
        modelBuilder.Entity<MonHoc>()
            .HasOne(monHoc => monHoc.DonVi)
            .WithMany(donVi => donVi.MonHocs)
            .HasForeignKey(monHoc => monHoc.DonViId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.ToTable("DiemDanhs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TrangThai).HasMaxLength(50);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.HasOne(e => e.SinhVien)
                .WithMany(s => s.DiemDanhs)
                .HasForeignKey(e => e.SinhVienId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Lop)
                .WithMany()
                .HasForeignKey(e => e.LopId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MonHocId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LichGiangDay>(entity =>
        {
            entity.ToTable("LichGiangDays");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Thu).HasMaxLength(20);
            entity.Property(e => e.PhongHoc).HasMaxLength(50);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.HocKy).HasMaxLength(100);
            entity.HasOne(e => e.GiangVien)
                .WithMany()
                .HasForeignKey(e => e.GiangVienId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.MonHoc)
                .WithMany()
                .HasForeignKey(e => e.MonHocId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Lop)
                .WithMany()
                .HasForeignKey(e => e.LopId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Diem>()
            .HasIndex(diem => new { diem.SinhVienId, diem.MonHocId })
            .IsUnique();
        modelBuilder.Entity<Diem>()
            .HasOne(diem => diem.SinhVien)
            .WithMany(sinhVien => sinhVien.Diems)
            .HasForeignKey(diem => diem.SinhVienId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Diem>()
            .HasOne(diem => diem.MonHoc)
            .WithMany()
            .HasForeignKey(diem => diem.MonHocId)
            .OnDelete(DeleteBehavior.Restrict);

        // === Cấu hình bổ sung từ schema QLSinhVienUTC ===
        modelBuilder.Entity<DonVi>(entity =>
        {
            entity.ToTable("DonVi");
            entity.HasIndex(e => e.MaDonVi, "UQ_DonVi_Ma").IsUnique();
            entity.Property(e => e.CreatedAt).HasPrecision(0).HasDefaultValueSql("(sysutcdatetime())", "DF_DonVi_CreatedAt");
            entity.Property(e => e.LoaiDonVi).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.MaDonVi).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.RowVer).IsRowVersion().IsConcurrencyToken();
            entity.Property(e => e.TenDonVi).HasMaxLength(200);
            entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("ACTIVE", "DF_DonVi_TrangThai");
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.Email).HasMaxLength(254).IsUnicode(false);
            entity.Property(e => e.DienThoai).HasMaxLength(20).IsUnicode(false);
            entity.HasOne(d => d.DonViCha).WithMany(p => p.InverseDonViCha).HasForeignKey(d => d.DonViChaId).HasConstraintName("FK_DonVi_Cha");
        });

        modelBuilder.Entity<NamHoc>(entity =>
        {
            entity.ToTable("NamHoc");
            entity.HasIndex(e => e.MaNamHoc, "UQ_NamHoc_Ma").IsUnique();
            entity.Property(e => e.MaNamHoc).HasMaxLength(9).IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("PLANNED", "DF_NamHoc_TrangThai");
        });

        modelBuilder.Entity<HocKy>(entity =>
        {
            entity.ToTable("HocKy");
            entity.HasIndex(e => new { e.NamHocId, e.MaHocKy }, "UQ_HocKy").IsUnique();
            entity.Property(e => e.MaHocKy).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TenHocKy).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("PLANNED", "DF_HocKy_TrangThai");
            entity.HasOne(d => d.NamHoc).WithMany(p => p.HocKies).HasForeignKey(d => d.NamHocId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_HocKy_NamHoc");
        });

        modelBuilder.Entity<KhoaTuyenSinh>(entity =>
        {
            entity.ToTable("KhoaTuyenSinh");
            entity.HasIndex(e => e.MaKhoa, "UQ_KhoaTuyenSinh_Ma").IsUnique();
            entity.Property(e => e.MaKhoa).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TenKhoa).HasMaxLength(100);
        });

        modelBuilder.Entity<PhongHoc>(entity =>
        {
            entity.ToTable("PhongHoc");
            entity.HasIndex(e => e.MaPhong, "UQ_PhongHoc_Ma").IsUnique();
            entity.Property(e => e.CoSo).HasMaxLength(100);
            entity.Property(e => e.LoaiPhong).HasMaxLength(20).IsUnicode(false).HasDefaultValue("LY_THUYET", "DF_Phong_Loai");
            entity.Property(e => e.MaPhong).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TenPhong).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("ACTIVE", "DF_Phong_TrangThai");
        });

        // modelBuilder.Entity<LopHanhChinh>(entity =>
        // {
        //     entity.ToTable("LopHanhChinh");
        //     entity.HasIndex(e => e.MaLop, "UQ_Lop_Ma").IsUnique();
        //     entity.Property(e => e.MaLop).HasMaxLength(30).IsUnicode(false);
        //     entity.Property(e => e.TenLop).HasMaxLength(150);
        //     entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("ACTIVE", "DF_Lop_TrangThai");
        //     entity.HasOne(d => d.ChuongTrinh).WithMany(p => p.LopHanhChinhs).HasForeignKey(d => d.ChuongTrinhId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Lop_CTD");
        //     entity.HasOne(d => d.CoVanGiangVien).WithMany(p => p.LopHanhChinhs).HasForeignKey(d => d.CoVanGiangVienId).HasConstraintName("FK_Lop_CoVan");
        //     entity.HasOne(d => d.KhoaTuyenSinh).WithMany(p => p.LopHanhChinhs).HasForeignKey(d => d.KhoaTuyenSinhId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Lop_Khoa");
        //     entity.HasOne(d => d.Nganh).WithMany(p => p.LopHanhChinhs).HasForeignKey(d => d.NganhId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_Lop_Nganh");
        // });

        // modelBuilder.Entity<LopHocPhan>(entity =>
        // {
        //     entity.ToTable("LopHocPhan");
        //     entity.HasIndex(e => new { e.HocKyId, e.MonHocId, e.TrangThai }, "IX_LHP_HocKy_Mon");
        //     entity.HasIndex(e => e.MaLopHocPhan, "UQ_LHP_Ma").IsUnique();
        //     entity.Property(e => e.MaLopHocPhan).HasMaxLength(30).IsUnicode(false);
        //     entity.Property(e => e.Nhom).HasMaxLength(20).IsUnicode(false).HasDefaultValue("01", "DF_LHP_Nhom");
        //     entity.Property(e => e.TrangThai).HasMaxLength(25).IsUnicode(false).HasDefaultValue("PLANNED", "DF_LHP_TrangThai");
        //     entity.HasOne(d => d.GiangVien).WithMany(p => p.LopHocPhans).HasForeignKey(d => d.GiangVienId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_LHP_GV");
        //     entity.HasOne(d => d.HocKy).WithMany(p => p.LopHocPhans).HasForeignKey(d => d.HocKyId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_LHP_HocKy");
        //     entity.HasOne(d => d.MonHoc).WithMany(p => p.LopHocPhans).HasForeignKey(d => d.MonHocId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_LHP_Mon");
        // });

        modelBuilder.Entity<LichHoc>(entity =>
        {
            entity.ToTable("LichHoc");
            // entity.HasIndex(e => e.LopHocPhanId, "IX_LichHoc_LHP");
            entity.Property(e => e.GioBatDau).HasPrecision(0);
            entity.Property(e => e.GioKetThuc).HasPrecision(0);
            // entity.HasOne(d => d.LopHocPhan).WithMany(p => p.LichHocs).HasForeignKey(d => d.LopHocPhanId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_LichHoc_LHP");
            entity.HasOne(d => d.PhongHoc).WithMany(p => p.LichHocs).HasForeignKey(d => d.PhongHocId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_LichHoc_Phong");
        });

        // modelBuilder.Entity<DangKyHocPhan>(entity =>
        // {
        //     entity.ToTable("DangKyHocPhan");
        //     entity.HasIndex(e => new { e.LopHocPhanId, e.TrangThai }, "IX_DK_LHP_TrangThai");
        //     entity.HasIndex(e => new { e.SinhVienId, e.TrangThai }, "IX_DK_SV_TrangThai");
        //     entity.HasIndex(e => new { e.SinhVienId, e.LopHocPhanId }, "UQ_DK_SV_LHP").IsUnique();
        //     entity.Property(e => e.GhiChu).HasMaxLength(300);
        //     entity.Property(e => e.LanHoc).HasDefaultValue((byte)1, "DF_DK_LanHoc");
        //     entity.Property(e => e.NgayDangKy).HasPrecision(0).HasDefaultValueSql("(sysutcdatetime())", "DF_DK_Ngay");
        //     entity.Property(e => e.RowVer).IsRowVersion().IsConcurrencyToken();
        //     entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("ENROLLED", "DF_DK_TrangThai");
        //     entity.HasOne(d => d.LopHocPhan).WithMany(p => p.DangKyHocPhans).HasForeignKey(d => d.LopHocPhanId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DK_LHP");
        //     entity.HasOne(d => d.SinhVien).WithMany(p => p.DangKyHocPhans).HasForeignKey(d => d.SinhVienId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DK_SV");
        // });

        modelBuilder.Entity<ThanhPhanDiem>(entity =>
        {
            entity.ToTable("ThanhPhanDiem");
            // entity.HasIndex(e => new { e.LopHocPhanId, e.MaThanhPhan }, "UQ_TPD_LHP_Ma").IsUnique();
            entity.Property(e => e.BatBuoc).HasDefaultValue(true, "DF_TPD_BatBuoc");
            entity.Property(e => e.DiemToiDa).HasDefaultValue(10m, "DF_TPD_DiemMax").HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MaThanhPhan).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.TenThanhPhan).HasMaxLength(100);
            entity.Property(e => e.TrongSo).HasColumnType("decimal(5, 2)");
            // entity.HasOne(d => d.LopHocPhan).WithMany(p => p.ThanhPhanDiems).HasForeignKey(d => d.LopHocPhanId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_TPD_LHP");
        });

        modelBuilder.Entity<DiemThanhPhan>(entity =>
        {
            entity.ToTable("DiemThanhPhan");
            entity.HasKey(e => new { e.DangKyId, e.ThanhPhanDiemId });
            entity.Property(e => e.Diem).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.NgayCapNhat).HasPrecision(0);
            entity.Property(e => e.NgayNhap).HasPrecision(0);
            entity.Property(e => e.NguoiNhap).HasMaxLength(450);
            entity.Property(e => e.NhanXet).HasMaxLength(500);
            // entity.HasOne(d => d.DangKy).WithMany(p => p.DiemThanhPhans).HasForeignKey(d => d.DangKyId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DTP_DK");
            entity.HasOne(d => d.ThanhPhanDiem).WithMany(p => p.DiemThanhPhans).HasForeignKey(d => d.ThanhPhanDiemId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_DTP_TPD");
        });

        modelBuilder.Entity<KetQuaHocTap>(entity =>
        {
            entity.ToTable("KetQuaHocTap");
            entity.HasKey(e => e.DangKyId);
            entity.Property(e => e.DangKyId).ValueGeneratedNever();
            entity.Property(e => e.DiemChu).HasMaxLength(2).IsUnicode(false);
            entity.Property(e => e.DiemHe4).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.DiemTongKet).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.NgayChot).HasPrecision(0);
            entity.Property(e => e.NguoiChot).HasMaxLength(450);
            entity.Property(e => e.PhienBan).HasDefaultValue(1, "DF_KQ_PhienBan");
            // entity.HasOne(d => d.DangKy).WithOne(p => p.KetQuaHocTap).HasForeignKey<KetQuaHocTap>(d => d.DangKyId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_KQ_DK");
        });

        // modelBuilder.Entity<HoaDonHocPhi>(entity =>
        // {
        //     entity.ToTable("HoaDonHocPhi");
        //     entity.HasIndex(e => e.SoHoaDon, "UQ_HoaDon_So").IsUnique();
        //     entity.Property(e => e.GhiChu).HasMaxLength(500);
        //     entity.Property(e => e.NgayLap).HasPrecision(0).HasDefaultValueSql("(sysutcdatetime())", "DF_HoaDon_Ngay");
        //     entity.Property(e => e.SoHoaDon).HasMaxLength(40).IsUnicode(false);
        //     entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("ISSUED", "DF_HoaDon_TrangThai");
        //     entity.HasOne(d => d.HocKy).WithMany(p => p.HoaDonHocPhis).HasForeignKey(d => d.HocKyId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_HoaDon_HocKy");
        //     entity.HasOne(d => d.SinhVien).WithMany(p => p.HoaDonHocPhis).HasForeignKey(d => d.SinhVienId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_HoaDon_SV");
        // });

        // modelBuilder.Entity<ChiTietHoaDonHocPhi>(entity =>
        // {
        //     entity.ToTable("ChiTietHoaDonHocPhi");
        //     entity.HasKey(e => e.ChiTietHoaDonId).HasName("PK_ChiTietHoaDon");
        //     entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
        //     entity.Property(e => e.LoaiKhoanThu).HasMaxLength(30).IsUnicode(false);
        //     entity.Property(e => e.MoTa).HasMaxLength(300);
        //     entity.Property(e => e.SoLuong).HasDefaultValue(1m, "DF_CTHD_SL").HasColumnType("decimal(10, 2)");
        //     entity.Property(e => e.SoTienGiam).HasColumnType("decimal(18, 2)");
        //     entity.HasOne(d => d.HoaDon).WithMany(p => p.ChiTietHoaDonHocPhis).HasForeignKey(d => d.HoaDonId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_CTHD_HD");
        // });

        // modelBuilder.Entity<ThanhToan>(entity =>
        // {
        //     entity.ToTable("ThanhToan");
        //     entity.HasIndex(e => new { e.HoaDonId, e.TrangThai }, "IX_ThanhToan_HD_TrangThai");
        //     entity.HasIndex(e => e.IdempotencyKey, "UQ_ThanhToan_Idempotency").IsUnique();
        //     entity.Property(e => e.GhiChu).HasMaxLength(500);
        //     entity.Property(e => e.IdempotencyKey).HasMaxLength(100).IsUnicode(false);
        //     entity.Property(e => e.MaGiaoDich).HasMaxLength(100).IsUnicode(false);
        //     entity.Property(e => e.NguonThanhToan).HasMaxLength(100);
        //     entity.Property(e => e.PhuongThuc).HasMaxLength(20).IsUnicode(false);
        //     entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");
        //     entity.Property(e => e.ThoiGianThanhToan).HasPrecision(0).HasDefaultValueSql("(sysutcdatetime())", "DF_ThanhToan_Time");
        //     entity.Property(e => e.TrangThai).HasMaxLength(20).IsUnicode(false).HasDefaultValue("PENDING", "DF_ThanhToan_TrangThai");
        //     entity.HasOne(d => d.HoaDon).WithMany(p => p.ThanhToans).HasForeignKey(d => d.HoaDonId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_ThanhToan_HD");
        // });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

