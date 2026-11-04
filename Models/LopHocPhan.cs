using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class LopHocPhan
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

    [Range(1, 12, ErrorMessage = "Tiết bắt đầu từ 1 đến 12")]
    [Display(Name = "Tiết bắt đầu")]
    public int TietBatDau { get; set; } = 1;

    [Range(1, 6, ErrorMessage = "Số tiết từ 1 đến 6")]
    [Display(Name = "Số tiết")]
    public int SoTiet { get; set; } = 3;

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
}
