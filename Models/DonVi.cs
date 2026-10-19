namespace BTL_StudentMGMT.Models;

public class DonVi
{
    public int Id { get; set; }
    public string TenDonVi { get; set; } = "";
    public string LoaiDonVi { get; set; } = "";
    public List<Nganh> Nganhs { get; set; } = new();
    public List<ChuongTrinhDaoTao> ChuongTrinhQuanLy { get; set; } = new();
    public List<ChuongTrinhDaoTao> ChuongTrinhChuyenMon { get; set; } = new();
}
