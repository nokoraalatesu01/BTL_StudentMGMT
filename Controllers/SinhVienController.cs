using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "SinhVien")]
public class SinhVienController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SinhVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = await LoadCurrentStudentAsync();
        return model == null ? View("Unlinked") : View(model);
    }

    public async Task<IActionResult> HoSo()
    {
        var model = await LoadCurrentStudentAsync();
        return model == null ? View("Unlinked") : View(model);
    }

    private async Task<SinhVienPortalViewModel?> LoadCurrentStudentAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return null;
        }

        var student = await _db.SinhViens
            .AsNoTracking()
            .Include(item => item.Lop)
                .ThenInclude(lop => lop!.Nganh)
            .FirstOrDefaultAsync(item => item.UserId == userId);

        if (student == null)
        {
            return null;
        }

        var classmates = student.Lop == null
            ? (int?)null
            : await _db.SinhViens.CountAsync(item => item.LopId == student.LopId);

        return new SinhVienPortalViewModel
        {
            UserName = User.Identity?.Name ?? "",
            HoTen = student.HoTen,
            MaSinhVien = student.MaSinhVien,
            NgaySinh = student.NgaySinh,
            GioiTinh = student.GioiTinh,
            Email = student.Email,
            SoDienThoai = student.SoDienThoai,
            DiaChi = student.DiaChi,
            TenLop = student.Lop?.TenLop,
            MaNganh = student.Lop?.Nganh?.MaNganh,
            TenNganh = student.Lop?.Nganh?.TenNganh,
            TenKhoa = student.Lop?.Nganh?.TenKhoa,
            SiSoLop = classmates
        };
    }
}
