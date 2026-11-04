namespace BTL_StudentMGMT.Models;

public class BieuPhiHocPhi
{
    public int BieuPhiId { get; set; }
    public int? ChuongTrinhId { get; set; }
    public int NamHocId { get; set; }
    public decimal DonGiaTinChi { get; set; }
    public string LoaiTien { get; set; } = "VND";
    public DateOnly HieuLucTu { get; set; }
    public DateOnly? HieuLucDen { get; set; }
}
