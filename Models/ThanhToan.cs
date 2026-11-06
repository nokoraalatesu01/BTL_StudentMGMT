using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class ThanhToan
{
    [Key]
    public long ThanhToanId { get; set; }
    public long HoaDonId { get; set; }
    public string? MaGiaoDich { get; set; }
    public decimal SoTien { get; set; }
    public DateTime ThoiGianThanhToan { get; set; }
    public string PhuongThuc { get; set; } = "";
    public string TrangThai { get; set; } = "PENDING";
    public string? NguonThanhToan { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? GhiChu { get; set; }
    public string? AnhMinhChung { get; set; }
    public HoaDonHocPhi HoaDon { get; set; } = null!;
}
