namespace BTL_StudentMGMT.Models;

public class SinhVienPortalViewModel
{
    public string UserName { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string MaSinhVien { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public string? TenLop { get; set; }
    public string? MaNganh { get; set; }
    public string? TenNganh { get; set; }
    public string? TenKhoa { get; set; }
    public int? SiSoLop { get; set; }
}
