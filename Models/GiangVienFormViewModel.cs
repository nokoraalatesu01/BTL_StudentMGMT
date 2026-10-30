using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BTL_StudentMGMT.Models;

public class GiangVienFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nhập mã giảng viên")]
    [StringLength(30, ErrorMessage = "Mã giảng viên không được vượt quá 30 ký tự")]
    [Display(Name = "Mã giảng viên")]
    public string MaGiangVien { get; set; } = "";

    [Required(ErrorMessage = "Nhập họ tên")]
    [StringLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Display(Name = "Đơn vị / Khoa trực thuộc")]
    public int? DonViId { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự")]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? HinhAnh { get; set; }

    [Display(Name = "Tải lên ảnh mới")]
    public IFormFile? FileAnh { get; set; }
}
