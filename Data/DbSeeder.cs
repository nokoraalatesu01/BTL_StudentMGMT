using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "GiangVien", "SinhVien" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUser = await CreateUser(userManager, "admin", "Quản trị viên", "Admin@123", "Admin");
        var giangVienUser = await CreateUser(userManager, "gv01", "Nguyễn Văn A", "GiangVien@123", "GiangVien");
        var sinhVienUser = await CreateUser(userManager, "sv01", "Trần Thị B", "SinhVien@123", "SinhVien");

        if (!db.Nganhs.Any())
        {
            var nganh = new Nganh { MaNganh = "CNTT", TenNganh = "Công nghệ thông tin" };
            nganh.Lops.Add(new Lop { TenLop = "CNTT01" });
            db.Nganhs.Add(nganh);
            await db.SaveChangesAsync();
        }

        if (giangVienUser != null && !db.GiangViens.Any())
        {
            db.GiangViens.Add(new GiangVien { MaGiangVien = "GV01", HoTen = "Nguyễn Văn A", UserId = giangVienUser.Id });
        }

        if (sinhVienUser != null && !db.SinhViens.Any())
        {
            db.SinhViens.Add(new SinhVien
            {
                MaSinhVien = "SV01",
                HoTen = "Trần Thị B",
                LopId = db.Lops.FirstOrDefault()?.Id,
                UserId = sinhVienUser.Id
            });
        }

        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser?> CreateUser(
        UserManager<ApplicationUser> userManager,
        string userName,
        string hoTen,
        string password,
        string role)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user != null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            UserName = userName,
            HoTen = hoTen,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return null;
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
