using System;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin,PGV")]
public class QuanLyDiemDanhController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<QuanLyDiemDanhController> _logger;

    public QuanLyDiemDanhController(
        ApplicationDbContext db,
        IAttendanceService attendanceService,
        ILogger<QuanLyDiemDanhController> logger)
    {
        _db = db;
        _attendanceService = attendanceService;
        _logger = logger;
    }

    // 1. Danh sách lớp học phần và tổng quan chuyên cần
    public async Task<IActionResult> Index(int? hocKyId, int? monHocId, int? giangVienId, string? tuKhoa, int page = 1)
    {
        var model = await _attendanceService.GetAdminIndexAsync(hocKyId, monHocId, giangVienId, tuKhoa, page, pageSize: 15);
        return View(model);
    }

    // 2. Chi tiết ma trận điểm danh của một lớp học phần
    public async Task<IActionResult> ChiTiet(int lopHocPhanId, DateTime? tuNgay = null, DateTime? denNgay = null)
    {
        try
        {
            var matrix = await _attendanceService.GetClassMatrixAsync(lopHocPhanId, tuNgay, denNgay, User);
            return View(matrix);
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // 3. Sửa buổi điểm danh (Admin/PGV không giới hạn cửa sổ ngày) - GET
    [HttpGet]
    public async Task<IActionResult> SuaBuoi(int lopHocPhanId, DateTime ngay)
    {
        var form = await _attendanceService.GetDiemDanhFormAsync(lopHocPhanId, ngay, User);
        if (form == null)
        {
            TempData["Error"] = "Không thể tải dữ liệu buổi điểm danh.";
            return RedirectToAction(nameof(ChiTiet), new { lopHocPhanId });
        }

        return View(form);
    }

    // 4. Lưu sửa buổi điểm danh - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuaBuoi(DiemDanhFormViewModel model)
    {
        if (!model.LopHocPhanId.HasValue || model.LopHocPhanId.Value <= 0)
        {
            TempData["Error"] = "Lớp học phần không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var res = await _attendanceService.SaveDiemDanhAsync(model, User);
        if (!res.Success)
        {
            TempData["Error"] = res.Message;
            // Tải lại form và hiển thị lỗi
            var form = await _attendanceService.GetDiemDanhFormAsync(model.LopHocPhanId.Value, model.NgayDiemDanh, User);
            if (form != null)
            {
                // Giữ lại trạng thái người dùng đã chọn
                if (model.DanhSachSinhVien != null)
                {
                    var map = model.DanhSachSinhVien.ToDictionary(s => s.SinhVienId);
                    foreach (var sv in form.DanhSachSinhVien)
                    {
                        if (map.TryGetValue(sv.SinhVienId, out var posted))
                        {
                            sv.TrangThai = posted.TrangThai;
                            sv.GhiChu = posted.GhiChu;
                        }
                    }
                }
                return View(form);
            }
            return RedirectToAction(nameof(ChiTiet), new { lopHocPhanId = model.LopHocPhanId.Value });
        }

        TempData["Msg"] = res.Message;
        return RedirectToAction(nameof(ChiTiet), new { lopHocPhanId = model.LopHocPhanId.Value });
    }

    // 5. Xuất Excel ma trận điểm danh (ClosedXML)
    [HttpGet]
    public async Task<IActionResult> ExportExcel(int lopHocPhanId, DateTime? tuNgay = null, DateTime? denNgay = null)
    {
        try
        {
            var bytes = await _attendanceService.ExportExcelMatrixAsync(lopHocPhanId, tuNgay, denNgay, User);
            var lhp = await _db.LopHocPhans.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lopHocPhanId);
            var fileName = $"DiemDanh_{lhp?.MaLopHocPhan ?? lopHocPhanId.ToString()}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi khi xuất file Excel: " + ex.Message;
            return RedirectToAction(nameof(ChiTiet), new { lopHocPhanId });
        }
    }
}
