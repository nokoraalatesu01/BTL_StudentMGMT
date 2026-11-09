using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_StudentMGMT.Models;

public class LopHocPhan : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã lớp học phần")]
    [StringLength(30, ErrorMessage = "Mã lớp học phần không quá 30 ký tự")]
    [Display(Name = "Mã lớp học phần")]
    public string MaLopHocPhan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên lớp học phần")]
    [StringLength(200, ErrorMessage = "Tên lớp học phần không quá 200 ký tự")]
    [Display(Name = "Tên lớp học phần")]
    public string TenLopHocPhan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn môn học")]
    [Display(Name = "Môn học")]
    public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }

    [Display(Name = "Học kỳ")]
    public int? HocKyId { get; set; }
    public HocKy? HocKy { get; set; }

    [Display(Name = "Giảng viên phụ trách")]
    public int? GiangVienId { get; set; }
    public GiangVien? GiangVien { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nhóm lớp")]
    [StringLength(20)]
    [Display(Name = "Nhóm lớp")]
    public string Nhom { get; set; } = "01";

    [Range(1, 200, ErrorMessage = "Sĩ số tối đa từ 1 đến 200")]
    [Display(Name = "Sĩ số tối đa")]
    public int SiSoToiDa { get; set; } = 60;

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "OPEN"; // OPEN (Đang mở), CLOSED (Đã khóa đăng ký), FINISHED (Đã kết thúc)

    // Lịch học của lớp học phần
    [StringLength(20)]
    [Display(Name = "Thứ trong tuần")]
    public string Thu { get; set; } = "Thứ 2"; // Thứ 2, Thứ 3, ...

    [Range(1, 10, ErrorMessage = "Tiết bắt đầu từ 1 đến 10")]
    [Display(Name = "Tiết bắt đầu")]
    public int TietBatDau { get; set; } = 1;

    [Range(2, 2, ErrorMessage = "Số tiết phải bằng 2 (mỗi lớp học phần đúng 1 ca)")]
    [Display(Name = "Số tiết")]
    public int SoTiet { get; set; } = 2;

    [NotMapped]
    [Required(ErrorMessage = "Vui lòng chọn ca học")]
    [Range(1, 5, ErrorMessage = "Ca học phải từ Ca 1 đến Ca 5")]
    [Display(Name = "Ca học")]
    public int Ca
    {
        get => CaHocHelper.GetCaTuTiet(TietBatDau);
        set
        {
            var (start, count) = CaHocHelper.GetTietTuCa(value);
            TietBatDau = start;
            SoTiet = count;
        }
    }

    [StringLength(50)]
    [Display(Name = "Phòng học")]
    public string PhongHoc { get; set; } = "P.302-A1";

    [StringLength(255)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    // Navigation collections
    public virtual ICollection<DangKyHocPhan> DangKyHocPhans { get; set; } = new List<DangKyHocPhan>();
    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();
    public virtual ICollection<Diem> Diems { get; set; } = new List<Diem>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!CaHocHelper.IsSingleCa(TietBatDau, SoTiet))
        {
            yield return new ValidationResult(
                "Lớp học phần phải học đúng 1 ca chuẩn (2 tiết, tiết bắt đầu thuộc {1, 3, 5, 7, 9}).",
                new[] { nameof(TietBatDau), nameof(SoTiet) });
        }
    }
}
