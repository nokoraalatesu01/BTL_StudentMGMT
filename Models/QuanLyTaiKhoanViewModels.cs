using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BTL_StudentMGMT.Models;

public class TaiKhoanListViewModel
{
    public List<TaiKhoanListItemViewModel> TaiKhoans { get; set; } = new();
}

public class TaiKhoanListItemViewModel
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? HoTen { get; set; }
    public string VaiTro { get; set; } = "";
    public string? HoSoLienKet { get; set; }
    public bool IsLocked { get; set; }
    public bool IsCurrentUser { get; set; }
}

public class TaoTaiKhoanViewModel
{
    [Required(ErrorMessage = "Chọn loại hồ sơ")]
    [RegularExpression("SinhVien|GiangVien", ErrorMessage = "Loại hồ sơ không hợp lệ")]
    [Display(Name = "Loại hồ sơ")]
    public string LoaiHoSo { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Chọn hồ sơ cần tạo tài khoản")]
    [Display(Name = "Hồ sơ")]
    public int HoSoId { get; set; }

    [Required(ErrorMessage = "Nhập tên đăng nhập")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 256 ký tự")]
    [Display(Name = "Tên đăng nhập")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu ban đầu")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu ban đầu")]
    public string Password { get; set; } = "";

    public List<SelectListItem> LoaiHoSoOptions { get; set; } = new();
    public List<SelectListItem> HoSoOptions { get; set; } = new();
}

public class DatLaiMatKhauViewModel
{
    public string UserId { get; set; } = "";

    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu mới")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = "";
}
