using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BTL_StudentMGMT.Models;

public class BuoiHocSelectOption
{
    public DateTime NgayHoc { get; set; }
    public string Text { get; set; } = "";
    public bool DaDiemDanh { get; set; }
}

public class DiemDanhItemViewModel
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? TenLopHanhChinh { get; set; }
    public string? TrangThai { get; set; } // null nếu chưa chọn
    public string? GhiChu { get; set; }
}

public class DiemDanhFormViewModel
{
    // Hỗ trợ điểm danh theo Lớp học phần (chuẩn tín chỉ)
    public int? LopHocPhanId { get; set; }
    public string? MaLopHocPhan { get; set; }
    public string? TenLopHocPhan { get; set; }
    public string? Thu { get; set; }
    public string? PhongHoc { get; set; }
    public string? KhungGio { get; set; }

    // Giữ lại để tương thích ngược dữ liệu cũ
    public int LopId { get; set; }
    public int MonHocId { get; set; }

    public DateTime NgayDiemDanh { get; set; } = DateTime.Today;
    public bool CanEdit { get; set; } = true;
    public string? EditRestrictionReason { get; set; }

    public List<BuoiHocSelectOption> DanhSachBuoiHoc { get; set; } = new();
    public List<DiemDanhItemViewModel> DanhSachSinhVien { get; set; } = new();
}

// =========================================================================
// PHASE 2 VIEWMODELS: G8, G9, G10, G11, G12
// =========================================================================

public class StudentSessionDetailDto
{
    public DateTime NgayDiemDanh { get; set; }
    public string Thu { get; set; } = "";
    public string TrangThai { get; set; } = "";
    public string TenTrangThai { get; set; } = "";
    public string BadgeClass { get; set; } = "";
    public string? GhiChu { get; set; }
}

public class StudentClassAttendanceItemDto
{
    public int LopHocPhanId { get; set; }
    public string MaLopHocPhan { get; set; } = "";
    public string TenLopHocPhan { get; set; } = "";
    public string TenMon { get; set; } = "";
    public int SoTinChi { get; set; }
    public string TenGiangVien { get; set; } = "";

    public int TongBuoi { get; set; }
    public int Present { get; set; }
    public int Late { get; set; }
    public int Excused { get; set; }
    public int Absent { get; set; }
    public double PhanTramVang { get; set; }
    public AttendanceWarningLevel MucCanhBao { get; set; }
    public string TenMucCanhBao { get; set; } = "Bình thường";
    public string BadgeClass { get; set; } = "";

    public double? DiemGoiY { get; set; }
    public string? DienGiaiGoiY { get; set; }

    public List<StudentSessionDetailDto> DanhSachBuoiHoc { get; set; } = new();
}

public class StudentAttendanceReportViewModel
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public int? SelectedHocKyId { get; set; }
    public List<SelectListItem> DanhSachHocKy { get; set; } = new();
    public List<StudentClassAttendanceItemDto> DanhSachLop { get; set; } = new();
}

public class StudentMatrixRowDto
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? TenLopHanhChinh { get; set; }

    public Dictionary<DateTime, string> AttendanceByDate { get; set; } = new();

    public int TongBuoi { get; set; }
    public int Present { get; set; }
    public int Late { get; set; }
    public int Excused { get; set; }
    public int Absent { get; set; }
    public double PhanTramVang { get; set; }
    public AttendanceWarningLevel MucCanhBao { get; set; }
    public string TenMucCanhBao { get; set; } = "Bình thường";
    public string BadgeClass { get; set; } = "";

    public double? DiemGoiY { get; set; }
    public string? DienGiaiGoiY { get; set; }
}

public class ClassAttendanceMatrixViewModel
{
    public int LopHocPhanId { get; set; }
    public string MaLopHocPhan { get; set; } = "";
    public string TenLopHocPhan { get; set; } = "";
    public string TenMon { get; set; } = "";
    public int SoTinChi { get; set; }
    public string TenGiangVien { get; set; } = "";
    public int SiSo { get; set; }

    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public int TongBuoiCuaLop { get; set; }

    public List<DateTime> Dates { get; set; } = new();
    public List<StudentMatrixRowDto> Students { get; set; } = new();
    public List<StudentMatrixRowDto> NguyCoStudents { get; set; } = new();
    public List<StudentMatrixRowDto> CanhBaoStudents { get; set; } = new();

    public bool CanEdit { get; set; } = true;
}

public class QuanLyDiemDanhLhpItemDto
{
    public int LopHocPhanId { get; set; }
    public string MaLopHocPhan { get; set; } = "";
    public string TenLopHocPhan { get; set; } = "";
    public string TenMon { get; set; } = "";
    public int SoTinChi { get; set; }
    public string TenGiangVien { get; set; } = "";
    public string TenHocKy { get; set; } = "";
    public int SiSo { get; set; }
    public int TongBuoiDaDiemDanh { get; set; }
    public int SoSinhVienNguyCo { get; set; }
    public int SoSinhVienCanhBao { get; set; }
}

public class QuanLyDiemDanhIndexViewModel
{
    public string? TuKhoa { get; set; }
    public int? HocKyId { get; set; }
    public int? MonHocId { get; set; }
    public int? GiangVienId { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }

    public List<SelectListItem> DanhSachHocKy { get; set; } = new();
    public List<SelectListItem> DanhSachMonHoc { get; set; } = new();
    public List<SelectListItem> DanhSachGiangVien { get; set; } = new();
    public List<QuanLyDiemDanhLhpItemDto> DanhSachLop { get; set; } = new();
}
