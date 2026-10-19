namespace BTL_StudentMGMT.Models;

public class Nganh
{
    public int Id { get; set; }
    public string MaNganh { get; set; } = "";
    public string TenNganh { get; set; } = "";
    public string TenKhoa { get; set; } = "";
    public int? DonViId { get; set; }
    public DonVi? DonVi { get; set; }
    public bool TichHopCuNhanKySu { get; set; }
    public List<Lop> Lops { get; set; } = new();
    public List<ChuongTrinhDaoTao> ChuongTrinhs { get; set; } = new();
}
