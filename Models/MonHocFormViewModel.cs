using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class MonHocFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã môn học")]
    [StringLength(50, ErrorMessage = "Mã môn học không được vượt quá 50 ký tự")]
    [Display(Name = "Mã môn học")]
    public string MaMon { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập tên môn học")]
    [StringLength(255, ErrorMessage = "Tên môn học không được vượt quá 255 ký tự")]
    [Display(Name = "Tên môn học")]
    public string TenMon { get; set; } = "";

    [Range(1, 6, ErrorMessage = "Số tín chỉ phải từ 1 đến 6")]
    [Display(Name = "Số tín chỉ")]
    public int SoTinChi { get; set; } = 3;
}
