namespace BTL_StudentMGMT.Models;

public class SinhVien
{
    public int Id { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public int? LopId { get; set; }
    public Lop? Lop { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
