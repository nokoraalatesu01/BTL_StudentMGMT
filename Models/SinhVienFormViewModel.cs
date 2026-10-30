using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BTL_StudentMGMT.Models;

public class SinhVienFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nhập mã sinh viên")]
    [Display(Name = "Mã sinh viên")]
    public string MaSinhVien { get; set; } = "";

    [Required(ErrorMessage = "Nhập họ tên")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime? NgaySinh { get; set; }

    [Display(Name = "Giới tính")]
    public string? GioiTinh { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [Display(Name = "Lớp")]
    public int? LopId { get; set; }

    [Display(Name = "Khóa / Năm nhập học")]
    [StringLength(50, ErrorMessage = "Khóa / năm nhập học không được vượt quá 50 ký tự")]
    public string? NienKhoa { get; set; }

    [Display(Name = "Trạng thái học tập")]
    [StringLength(50, ErrorMessage = "Trạng thái học tập không được vượt quá 50 ký tự")]
    public string? TrangThai { get; set; }

    [Display(Name = "Điểm GPA (Hệ 10)")]
    public double? Gpa { get; set; }

    public string? XepLoai { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? HinhAnh { get; set; }

    [Display(Name = "Tải lên ảnh mới")]
    public IFormFile? FileAnh { get; set; }
}
