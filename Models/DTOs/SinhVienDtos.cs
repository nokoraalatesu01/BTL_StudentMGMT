using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models.DTOs;

/// <summary>
/// DTO trả về thông tin sinh viên qua RESTful Web API (tránh lỗi tham chiếu vòng lặp Entity Framework)
/// </summary>
public class SinhVienDto
{
    public int Id { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public int? LopId { get; set; }
    public string? TenLop { get; set; }
    public string? Nganh { get; set; }
    public string? NienKhoa { get; set; }
    public string? TrangThai { get; set; }
    public string? HinhAnh { get; set; }
    public double? Gpa { get; set; }
}

/// <summary>
/// DTO nhận dữ liệu khi thêm mới sinh viên qua POST /api/sinhvien
/// </summary>
public class CreateSinhVienDto
{
    [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
    [StringLength(20, ErrorMessage = "Mã sinh viên tối đa 20 ký tự")]
    public string MaSinhVien { get; set; } = "";

    [Required(ErrorMessage = "Họ tên sinh viên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    public string HoTen { get; set; } = "";

    public DateTime? NgaySinh { get; set; }

    [StringLength(10)]
    public string? GioiTinh { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [StringLength(255)]
    public string? DiaChi { get; set; }

    public int? LopId { get; set; }

    [StringLength(50)]
    public string? NienKhoa { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; } = "Đang học";
}

/// <summary>
/// DTO nhận dữ liệu khi cập nhật sinh viên qua PUT /api/sinhvien/{id}
/// </summary>
public class UpdateSinhVienDto
{
    [Required(ErrorMessage = "Họ tên sinh viên là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
    public string HoTen { get; set; } = "";

    public DateTime? NgaySinh { get; set; }

    [StringLength(10)]
    public string? GioiTinh { get; set; }

    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20)]
    public string? SoDienThoai { get; set; }

    [StringLength(255)]
    public string? DiaChi { get; set; }

    public int? LopId { get; set; }

    [StringLength(50)]
    public string? NienKhoa { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }
}

/// <summary>
/// DTO môn học trả về qua RESTful API
/// </summary>
public class MonHocDto
{
    public int Id { get; set; }
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public int SoTinChi { get; set; }
    public int? DonViId { get; set; }
    public string? TenDonVi { get; set; }
}

