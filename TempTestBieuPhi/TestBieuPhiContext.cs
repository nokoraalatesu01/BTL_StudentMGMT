using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.TempTestBieuPhi;

public partial class TestBieuPhiContext : DbContext
{
    public TestBieuPhiContext(DbContextOptions<TestBieuPhiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BieuPhiHocPhi> BieuPhiHocPhis { get; set; }

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
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
