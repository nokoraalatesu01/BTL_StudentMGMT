using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_StudentMGMT.Models;

public class Diem
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Sinh viên")]
    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }

    [Required]
    [Display(Name = "Môn học")]
    public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }

    // Liên kết với Lớp học phần đào tạo tín chỉ
    [Display(Name = "Lớp học phần")]
    public int? LopHocPhanId { get; set; }
    public LopHocPhan? LopHocPhan { get; set; }

    // Liên kết với Học kỳ
    [Display(Name = "Học kỳ")]
    public int? HocKyId { get; set; }
    public HocKy? HocKy { get; set; }

    // Phân loại hình thức học theo quy chế tín chỉ
    // HOC_LAN_DAU: Học lần đầu
    // HOC_LAI: Học lại do không đạt (F)
    // HOC_CAI_THIEN: Học lại để nâng điểm GPA (đã đạt D, D+, C,...)
    [Required]
    [StringLength(30)]
    [Display(Name = "Loại học")]
    public string LoaiHoc { get; set; } = "HOC_LAN_DAU";

    // Số lần học của môn này (1, 2, 3...)
    [Range(1, 10)]
    [Display(Name = "Lần học")]
    public int LanHoc { get; set; } = 1;

    // Số lần thi kết thúc học phần trong cùng 1 lần học (1: thi lần 1, 2: thi lại lần 2)
    [Range(1, 5)]
    [Display(Name = "Lần thi")]
    public int LanThi { get; set; } = 1;

    // Các thành phần điểm
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm chuyên cần (10%)")]
    public double DiemChuyenCan { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Điểm giữa kỳ (30%)")]
    public double DiemGiuaKy { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Thi kết thúc HP lần 1 (60%)")]
    public double DiemCuoiKy { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    [Display(Name = "Thi kết thúc HP lần 2 (nếu thi lại)")]
    public double? DiemCuoiKyLan2 { get; set; }

    // Cờ đánh dấu bản ghi điểm này có được chọn để tính điểm trung bình chung tích lũy (GPA) hay không
    // Quy tắc tín chỉ: Mỗi môn học chỉ có DUY NHẤT 1 bản ghi có SuDungTinhTichLuy = true (thường là lần học có điểm cao nhất)
    [Display(Name = "Tính tích lũy GPA")]
    public bool SuDungTinhTichLuy { get; set; } = true;

    // Trạng thái kết quả: DAT (>= 4.0), KHONG_DAT (< 4.0), CHUA_HOAN_THANH
    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "DAT";

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    [Display(Name = "Ngày cập nhật")]
    public DateTime? NgayCapNhat { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    [Display(Name = "Người cập nhật")]
    public string? NguoiCapNhat { get; set; }

    // Điểm thi cuối kỳ thực tế được áp dụng (lần 2 nếu thi lại, ngược lại lần 1)
    [NotMapped]
    public double DiemCuoiKyApDung => (LanThi == 2 && DiemCuoiKyLan2.HasValue) ? DiemCuoiKyLan2.Value : DiemCuoiKy;

    // Tính điểm tổng kết = CC * 10% + GK * 30% + CK * 60%
    public double DiemTongKet => Math.Round(DiemChuyenCan * 0.1 + DiemGiuaKy * 0.3 + DiemCuoiKyApDung * 0.6, 2);

    // Điểm hệ 4 theo quy chế tín chỉ
    [NotMapped]
    public double DiemHe4
    {
        get
        {
            if (DiemTongKet >= 8.5) return 4.0;
            if (DiemTongKet >= 8.0) return 3.5;
            if (DiemTongKet >= 7.0) return 3.0;
            if (DiemTongKet >= 6.5) return 2.5;
            if (DiemTongKet >= 5.5) return 2.0;
            if (DiemTongKet >= 5.0) return 1.5;
            if (DiemTongKet >= 4.0) return 1.0;
            return 0.0;
        }
    }

    // Xếp loại điểm chữ theo quy chế tín chỉ
    public string DiemChu
    {
        get
        {
            if (DiemTongKet >= 9.0) return "A+";
            if (DiemTongKet >= 8.5) return "A";
            if (DiemTongKet >= 8.0) return "B+";
            if (DiemTongKet >= 7.0) return "B";
            if (DiemTongKet >= 6.5) return "C+";
            if (DiemTongKet >= 5.5) return "C";
            if (DiemTongKet >= 5.0) return "D+";
            if (DiemTongKet >= 4.0) return "D";
            return "F";
        }
    }

    [NotMapped]
    public string TenLoaiHoc
    {
        get
        {
            return LoaiHoc switch
            {
                "HOC_LAN_DAU" => "Học lần đầu",
                "HOC_LAI" => "Học lại",
                "HOC_CAI_THIEN" => "Học cải thiện",
                _ => "Học phần"
            };
        }
    }

    [NotMapped]
    public bool IsDat => DiemTongKet >= 4.0;
}
