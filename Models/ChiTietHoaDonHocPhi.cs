namespace BTL_StudentMGMT.Models;

public class ChiTietHoaDonHocPhi
{
    public long ChiTietHoaDonId { get; set; }
    public long HoaDonId { get; set; }
    public string LoaiKhoanThu { get; set; } = "HOCPHI";
    public string MoTa { get; set; } = "";
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal SoTienGiam { get; set; }
    public HoaDonHocPhi HoaDon { get; set; } = null!;
}
