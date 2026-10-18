using System.ComponentModel.DataAnnotations;

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
}
