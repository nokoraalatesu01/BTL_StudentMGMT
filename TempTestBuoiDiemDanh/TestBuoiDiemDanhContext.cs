using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.TempTestBuoiDiemDanh;

public partial class TestBuoiDiemDanhContext : DbContext
{
    public TestBuoiDiemDanhContext(DbContextOptions<TestBuoiDiemDanhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BuoiDiemDanh> BuoiDiemDanhs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuoiDiemDanh>(entity =>
        {
            entity.ToTable("BuoiDiemDanh");

            entity.Property(e => e.NguoiTao).HasMaxLength(450);
            entity.Property(e => e.NoiDung).HasMaxLength(200);
            entity.Property(e => e.ThoiGianBatDau).HasPrecision(0);
            entity.Property(e => e.ThoiGianKetThuc).HasPrecision(0);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
