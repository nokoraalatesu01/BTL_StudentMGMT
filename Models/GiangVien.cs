namespace BTL_StudentMGMT.Models;

public class GiangVien
{
    public int Id { get; set; }
    public string MaGiangVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
