namespace BTL_StudentMGMT.Models;

public class HoaDonHocPhi
{
    public long HoaDonId { get; set; }
    public string SoHoaDon { get; set; } = "";
    public int SinhVienId { get; set; }
    public int HocKyId { get; set; }
    public DateTime NgayLap { get; set; }
    public DateOnly? HanThanhToan { get; set; }
    public string TrangThai { get; set; } = "ISSUED";
    public string? GhiChu { get; set; }
    public SinhVien SinhVien { get; set; } = null!;
    public HocKy HocKy { get; set; } = null!;
    public ICollection<ChiTietHoaDonHocPhi> ChiTietHoaDonHocPhis { get; set; } = new List<ChiTietHoaDonHocPhi>();
    public ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
