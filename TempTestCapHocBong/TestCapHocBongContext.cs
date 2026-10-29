using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.TempTestCapHocBong;

public partial class TestCapHocBongContext : DbContext
{
    public TestCapHocBongContext(DbContextOptions<TestCapHocBongContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CapHocBong> CapHocBongs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
