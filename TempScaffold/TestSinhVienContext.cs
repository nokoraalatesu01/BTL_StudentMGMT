using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.TempScaffold;

public partial class TestSinhVienContext : DbContext
{
    public TestSinhVienContext(DbContextOptions<TestSinhVienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
