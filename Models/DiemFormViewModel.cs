using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class DiemFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Lớp học phần")]
    public int? LopHocPhanId { get; set; }
    public string? TenLopHocPhan { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn sinh viên")]
    [Display(Name = "Sinh viên")]
    public int SinhVienId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn môn học")]
    [Display(Name = "Môn học")]
    public int MonHocId { get; set; }

    [Display(Name = "Học kỳ")]
    public int? HocKyId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại học")]
    [Display(Name = "Loại học")]
    public string LoaiHoc { get; set; } = "HOC_LAN_DAU"; // HOC_LAN_DAU, HOC_LAI, HOC_CAI_THIEN

    [Range(1, 10, ErrorMessage = "Lần học từ 1 đến 10")]
    [Display(Name = "Lần học")]
    public int LanHoc { get; set; } = 1;

    [Range(1, 2, ErrorMessage = "Lần thi là 1 hoặc 2")]
    [Display(Name = "Lần thi")]
    public int LanThi { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng nhập điểm chuyên cần")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm chuyên cần (10%)")]
    public double? DiemChuyenCan { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập điểm giữa kỳ")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm giữa kỳ (30%)")]
    public double? DiemGiuaKy { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập điểm thi cuối kỳ lần 1")]
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Thi kết thúc HP lần 1 (60%)")]
    public double? DiemCuoiKy { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Thi kết thúc HP lần 2 (nếu thi lại)")]
    public double? DiemCuoiKyLan2 { get; set; }

    [Display(Name = "Sử dụng tính tích lũy GPA")]
    public bool SuDungTinhTichLuy { get; set; } = true;

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    // Thông tin hiển thị lịch sử các lần học trước (nếu có)
    public List<Diem>? LichSuDiemMonHoc { get; set; }
    public List<Diem>? LichSuDiem { get => LichSuDiemMonHoc; set => LichSuDiemMonHoc = value; }
}
