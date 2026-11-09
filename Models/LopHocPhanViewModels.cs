using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class LopHocPhanFormViewModel
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

    [Display(Name = "Học kỳ")]
    public int? HocKyId { get; set; }

    [Display(Name = "Giảng viên phụ trách")]
    public int? GiangVienId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nhóm lớp")]
    [StringLength(20)]
    [Display(Name = "Nhóm lớp")]
    public string Nhom { get; set; } = "01";

    [Range(1, 200, ErrorMessage = "Sĩ số tối đa từ 1 đến 200 sinh viên")]
    [Display(Name = "Sĩ số tối đa")]
    public int SiSoToiDa { get; set; } = 60;

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "OPEN"; // OPEN, CLOSED, FINISHED

    [Display(Name = "Thứ")]
    public string Thu { get; set; } = "Thứ 2";

    [Required(ErrorMessage = "Vui lòng chọn ca học")]
    [Range(1, 5, ErrorMessage = "Ca học phải từ Ca 1 đến Ca 5")]
    [Display(Name = "Ca học")]
    public int? Ca { get; set; } = 1;

    [Range(1, 10, ErrorMessage = "Tiết bắt đầu từ 1 đến 10")]
    [Display(Name = "Tiết bắt đầu")]
    public int TietBatDau { get; set; } = 1;

    [Range(2, 2, ErrorMessage = "Số tiết phải bằng 2 (mỗi lớp học phần đúng 1 ca)")]
    [Display(Name = "Số tiết")]
    public int SoTiet { get; set; } = 2;

    [StringLength(50)]
    [Display(Name = "Phòng học")]
    public string PhongHoc { get; set; } = "P.302-A1";

    [StringLength(255)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public class LopHocPhanListItemViewModel
{
    public int Id { get; set; }
    public string MaLopHocPhan { get; set; } = string.Empty;
    public string TenLopHocPhan { get; set; } = string.Empty;
    public string TenMon { get; set; } = string.Empty;
    public string MaMon { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public string? TenGiangVien { get; set; }
    public string? TenHocKy { get; set; }
    public string Nhom { get; set; } = "01";
    public int SiSoToiDa { get; set; }
    public int SiSoHienTai { get; set; }
    public string TrangThai { get; set; } = "OPEN";
    public string LichHoc { get; set; } = string.Empty;
    public string PhongHoc { get; set; } = string.Empty;
}

public class LopHocPhanSinhVienViewModel
{
    public LopHocPhan LopHocPhan { get; set; } = null!;
    public List<DangKyHocPhanItemViewModel> DanhSachSinhVien { get; set; } = new();
}

public class DangKyHocPhanItemViewModel
{
    public int DangKyId { get; set; }
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? TenLopHanhChinh { get; set; }
    public string? TenNganh { get; set; }
    public DateTime NgayDangKy { get; set; }
    public int LanHoc { get; set; } = 1;
    public string TrangThai { get; set; } = EnrollmentStatus.Enrolled;
    public double? DiemTongKet { get; set; }
    public string? DiemChu { get; set; }
}

public class SinhVienDangKyHocPhanViewModel
{
    public SinhVien SinhVien { get; set; } = null!;
    public List<LopHocPhanDangKyItemViewModel> LopHocPhansMo { get; set; } = new();
    public List<LopHocPhanDangKyItemViewModel> LopHocPhanDaDangKy { get; set; } = new();
    public int TongTinChiDaDangKy => LopHocPhanDaDangKy.Sum(x => x.SoTinChi);
}

public class LopHocPhanDangKyItemViewModel
{
    public int LopHocPhanId { get; set; }
    public string MaLopHocPhan { get; set; } = string.Empty;
    public string TenLopHocPhan { get; set; } = string.Empty;
    public string MaMon { get; set; } = string.Empty;
    public string TenMon { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public string? TenGiangVien { get; set; }
    public string LichHoc { get; set; } = string.Empty;
    public string PhongHoc { get; set; } = string.Empty;
    public int SiSoHienTai { get; set; }
    public int SiSoToiDa { get; set; }
    public bool DaDangKy { get; set; }
    public DateTime? NgayDangKy { get; set; }
}

public class ThoiKhoaBieuItemViewModel
{
    public int LopHocPhanId { get; set; }
    public string MaLopHocPhan { get; set; } = string.Empty;
    public string TenMon { get; set; } = string.Empty;
    public int SoTinChi { get; set; }
    public string? GiangVien { get; set; }
    public string Thu { get; set; } = string.Empty;
    public int TietBatDau { get; set; }
    public int SoTiet { get; set; }
    public int TietKetThuc => TietBatDau + SoTiet - 1;
    public string PhongHoc { get; set; } = string.Empty;
}
