using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

        modelBuilder.Entity<ChuongTrinhDaoTao>()
            .Property(chuongTrinh => chuongTrinh.MaChuongTrinh)
            .HasMaxLength(30);
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
            .HasIndex(giangVien => giangVien.MaGiangVien)
            .IsUnique();
        modelBuilder.Entity<GiangVien>()
            .HasIndex(giangVien => giangVien.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");
        modelBuilder.Entity<SinhVien>()
            .Property(sinhVien => sinhVien.NienKhoa)
            .HasMaxLength(50);
        modelBuilder.Entity<SinhVien>()
            .Property(sinhVien => sinhVien.TrangThai)
            .HasMaxLength(50);
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
    }
}

