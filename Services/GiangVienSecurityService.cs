using System.Security.Claims;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BTL_StudentMGMT.Services;

public class GiangVienSecurityService : IGiangVienSecurityService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GiangVienSecurityService> _logger;

    public GiangVienSecurityService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger<GiangVienSecurityService> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<GiangVien?> GetCurrentGiangVienAsync(ClaimsPrincipal user)
    {
        var appUser = await _userManager.GetUserAsync(user);
        if (appUser == null) return null;

        return await _db.GiangViens
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.UserId == appUser.Id);
    }

    public async Task<bool> CanAccessLopHocPhanAsync(ClaimsPrincipal user, int lopHocPhanId)
    {
        if (user.IsInRole("Admin") || user.IsInRole("PGV")) return true;
        if (!user.IsInRole("GiangVien")) return false;

        var giangVien = await GetCurrentGiangVienAsync(user);
        if (giangVien == null) return false;

        return await _db.LopHocPhans
            .AnyAsync(lhp => lhp.Id == lopHocPhanId && lhp.GiangVienId == giangVien.Id);
    }

    public async Task<bool> CanGradeStudentAsync(ClaimsPrincipal user, int lopHocPhanId, int sinhVienId)
    {
        if (user.IsInRole("Admin") || user.IsInRole("PGV")) return true;
        if (!user.IsInRole("GiangVien")) return false;

        var hasAccessLhp = await CanAccessLopHocPhanAsync(user, lopHocPhanId);
        if (!hasAccessLhp) return false;

        // Kiểm tra sinh viên có thực sự đăng ký lớp học phần đó không
        return await _db.DangKyHocPhans
            .AnyAsync(dk => dk.LopHocPhanId == lopHocPhanId && dk.SinhVienId == sinhVienId && dk.TrangThai == "ENROLLED");
    }

    public async Task<bool> CanModifyDiemAsync(ClaimsPrincipal user, int diemId)
    {
        if (user.IsInRole("Admin") || user.IsInRole("PGV")) return true;
        if (!user.IsInRole("GiangVien")) return false;

        var giangVien = await GetCurrentGiangVienAsync(user);
        if (giangVien == null) return false;

        var diem = await _db.Diems
            .Include(d => d.LopHocPhan)
            .FirstOrDefaultAsync(d => d.Id == diemId);

        if (diem == null) return false;

        // Nếu điểm gắn với lớp học phần: kiểm tra giảng viên phụ trách
        if (diem.LopHocPhanId.HasValue)
        {
            return diem.LopHocPhan != null && diem.LopHocPhan.GiangVienId == giangVien.Id;
        }

        return false;
    }

    public void LogSecurityViolation(ClaimsPrincipal user, string action, string resource, string details)
    {
        var username = user.Identity?.Name ?? "Anonymous";
        var roles = string.Join(",", user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value));
        _logger.LogWarning(
            "[SECURITY AUDIT] Unauthorized access attempt detected! User: '{Username}' | Roles: '{Roles}' | Action: '{Action}' | Resource: '{Resource}' | Details: '{Details}' | Timestamp: {Timestamp}",
            username, roles, action, resource, details, DateTime.UtcNow);
    }
}
