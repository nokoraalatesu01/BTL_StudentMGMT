using System.Security.Claims;
using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Services;

public interface IGiangVienSecurityService
{
    Task<GiangVien?> GetCurrentGiangVienAsync(ClaimsPrincipal user);
    Task<bool> CanAccessLopHocPhanAsync(ClaimsPrincipal user, int lopHocPhanId);
    Task<bool> CanGradeStudentAsync(ClaimsPrincipal user, int lopHocPhanId, int sinhVienId);
    Task<bool> CanModifyDiemAsync(ClaimsPrincipal user, int diemId);
    void LogSecurityViolation(ClaimsPrincipal user, string action, string resource, string details);
}
