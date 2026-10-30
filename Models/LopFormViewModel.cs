using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class LopFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên lớp")]
    [StringLength(60, ErrorMessage = "Tên lớp không được vượt quá 60 ký tự")]
    [Display(Name = "Tên lớp")]
    public string TenLop { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn ngành")]
    [Display(Name = "Ngành")]
    public int NganhId { get; set; }

    [Display(Name = "Chương trình đào tạo (tùy chọn)")]
    public int? ChuongTrinhDaoTaoId { get; set; }
}
