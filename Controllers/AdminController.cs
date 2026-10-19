using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
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
