using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class DiemFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn sinh viên")]
    [Display(Name = "Sinh viên")]
    public int SinhVienId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn môn học")]
    [Display(Name = "Môn học")]
    public int MonHocId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập điểm chuyên cần")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm chuyên cần (10%)")]
    public double DiemChuyenCan { get; set; } = 10;

    [Required(ErrorMessage = "Vui lòng nhập điểm giữa kỳ")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm giữa kỳ (30%)")]
    public double DiemGiuaKy { get; set; } = 8;

    [Required(ErrorMessage = "Vui lòng nhập điểm thi cuối kỳ")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm thi cuối kỳ (60%)")]
    public double DiemCuoiKy { get; set; } = 8.5;
}
