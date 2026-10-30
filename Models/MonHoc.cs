namespace BTL_StudentMGMT.Models;

public class MonHoc
{
    public int Id { get; set; }
    public string MaMon { get; set; } = "";
    public string TenMon { get; set; } = "";
    public int SoTinChi { get; set; } = 3;
    public int? DonViId { get; set; }
    public virtual DonVi? DonVi { get; set; }
}
