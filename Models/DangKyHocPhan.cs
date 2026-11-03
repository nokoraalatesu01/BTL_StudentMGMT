using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class DangKyHocPhan
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Sinh viên")]
    public int SinhVienId { get; set; }
    public virtual SinhVien SinhVien { get; set; } = null!;

    [Required]
    [Display(Name = "Lớp học phần")]
    public int LopHocPhanId { get; set; }
    public virtual LopHocPhan LopHocPhan { get; set; } = null!;

    [Display(Name = "Ngày đăng ký")]
    public DateTime NgayDangKy { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "ENROLLED"; // ENROLLED (Đã đăng ký), DROPPED (Đã hủy)

    [Range(1, 10)]
    [Display(Name = "Lần học")]
    public int LanHoc { get; set; } = 1;

    [StringLength(300)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}
