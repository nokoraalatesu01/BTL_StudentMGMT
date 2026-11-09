using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardViewModel
        {
            TongSinhVien = await _db.SinhViens.CountAsync(),
            TongGiangVien = await _db.GiangViens.CountAsync(),
            TongNganh = await _db.Nganhs.CountAsync(),
            TongLop = await _db.Lops.CountAsync(),
            TongMonHoc = await _db.MonHocs.CountAsync(),
            TongLopHocPhan = await _db.LopHocPhans.CountAsync(),
            TongDangKyHocPhan = await _db.DangKyHocPhans.CountAsync(dk => dk.TrangThai == EnrollmentStatus.Enrolled),
            TongTaiKhoan = await _db.Users.CountAsync(),
            SinhVienChuaCoLop = await _db.SinhViens.CountAsync(s => s.LopId == null),
            SinhVienChuaCoTaiKhoan = await _db.SinhViens.CountAsync(s => s.UserId == null),
            GiangVienChuaCoTaiKhoan = await _db.GiangViens.CountAsync(g => g.UserId == null),
            ThongKeTheoLop = await _db.Lops
                .Select(l => new ThongKeLop { TenLop = l.TenLop, SoSinhVien = l.SinhViens.Count })
                .OrderByDescending(x => x.SoSinhVien)
                .Take(10)
                .ToListAsync()
        };

        return View(vm);
    }
}
