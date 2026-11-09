using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Services;

public record SessionValidationResult(bool IsValid, string? ErrorMessage = null, bool IsOutsideEditWindow = false);

public record SaveAttendanceResult(bool Success, string Message, int UpdatedCount = 0);

public interface IAttendanceService
{
    /// <summary>
    /// Lấy danh sách các ngày học hợp lệ của một lớp học phần trong kỳ học
    /// </summary>
    Task<List<DateTime>> GetValidSessionDatesAsync(int lopHocPhanId);

    /// <summary>
    /// Kiểm tra tính hợp lệ của ngày điểm danh theo 4 điều kiện
    /// </summary>
    Task<SessionValidationResult> ValidateSessionDateAsync(int lopHocPhanId, DateTime date, ClaimsPrincipal user);

    /// <summary>
    /// Lấy dữ liệu ViewModel cho màn hình điểm danh
    /// </summary>
    Task<DiemDanhFormViewModel?> GetDiemDanhFormAsync(int lopHocPhanId, DateTime date, ClaimsPrincipal user);

    /// <summary>
    /// Lưu kết quả điểm danh cho lớp học phần (chặn N+1, validate toàn bộ sinh viên, audit log)
    /// </summary>
    Task<SaveAttendanceResult> SaveDiemDanhAsync(DiemDanhFormViewModel model, ClaimsPrincipal user);

    /// <summary>
    /// Xóa toàn bộ bản ghi điểm danh của sinh viên trong một lớp học phần (dùng khi hủy đăng ký / xóa SV)
    /// </summary>
    Task<int> DeleteAttendanceForStudentAsync(int lopHocPhanId, int sinhVienId);

    // =========================================================================
    // PHASE 2 METHODS
    // =========================================================================

    /// <summary>
    /// G8: Thống kê điểm danh cá nhân của sinh viên theo từng lớp học phần trong học kỳ
    /// </summary>
    Task<StudentAttendanceReportViewModel> GetStudentAttendanceReportAsync(int sinhVienId, int? hocKyId);

    /// <summary>
    /// G9 & G10: Ma trận điểm danh sinh viên x ngày của lớp học phần
    /// Kiểm tra quyền qua IGiangVienSecurityService (GV chỉ lớp mình, Admin/PGV mọi lớp)
    /// </summary>
    Task<ClassAttendanceMatrixViewModel> GetClassMatrixAsync(int lopHocPhanId, DateTime? tuNgay, DateTime? denNgay, ClaimsPrincipal user);

    /// <summary>
    /// G10: Xuất Excel ma trận điểm danh bằng ClosedXML
    /// </summary>
    Task<byte[]> ExportExcelMatrixAsync(int lopHocPhanId, DateTime? tuNgay, DateTime? denNgay, ClaimsPrincipal user);

    /// <summary>
    /// G12: Gợi ý điểm chuyên cần cho sinh viên trong lớp học phần
    /// </summary>
    Task<AttendanceCalculationResult?> GetStudentSuggestionAsync(int lopHocPhanId, int sinhVienId, ClaimsPrincipal user);

    /// <summary>
    /// G10: Màn hình danh sách lớp học phần cho Admin/PGV
    /// </summary>
    Task<QuanLyDiemDanhIndexViewModel> GetAdminIndexAsync(int? hocKyId, int? monHocId, int? giangVienId, string? search, int page = 1, int pageSize = 15);
}
