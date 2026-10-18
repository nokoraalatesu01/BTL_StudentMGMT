using System.ComponentModel.DataAnnotations;

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

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(256, ErrorMessage = "Email không được vượt quá 256 ký tự")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự")]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }
}
