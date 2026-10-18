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
}
