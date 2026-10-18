using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class NganhFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã ngành")]
    [StringLength(20, ErrorMessage = "Mã ngành không được vượt quá 20 ký tự")]
    [Display(Name = "Mã ngành")]
    public string MaNganh { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập tên ngành")]
    [StringLength(150, ErrorMessage = "Tên ngành không được vượt quá 150 ký tự")]
    [Display(Name = "Tên ngành")]
    public string TenNganh { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập tên khoa")]
    [StringLength(150, ErrorMessage = "Tên khoa không được vượt quá 150 ký tự")]
    [Display(Name = "Khoa")]
    public string TenKhoa { get; set; } = "";

    [Display(Name = "Đào tạo tích hợp Cử nhân - Kỹ sư")]
    public bool TichHopCuNhanKySu { get; set; }
}
