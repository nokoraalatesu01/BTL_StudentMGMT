using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BTL_StudentMGMT.Models;

public class CapNhatGiangVienViewModel
{
    public int Id { get; set; }

    [Display(Name = "Mã giảng viên")]
    public string MaGiangVien { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [Display(Name = "Họ và tên")]
    public string HoTen { get; set; } = "";

    [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Số điện thoại")]
    [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng số 0 và có 10-11 chữ số")]
    public string? SoDienThoai { get; set; }

    [Display(Name = "Mật khẩu mới (bỏ trống nếu không đổi)")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
    public string? MatKhauMoi { get; set; }

    [Display(Name = "Xác nhận mật khẩu mới")]
    [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string? XacNhanMatKhau { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? HinhAnh { get; set; }

    [Display(Name = "Tải lên ảnh mới")]
    public IFormFile? FileAnh { get; set; }
}
