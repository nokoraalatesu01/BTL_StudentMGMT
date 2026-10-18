namespace BTL_StudentMGMT.Models;

public class LopListItemViewModel
{
    public int Id { get; set; }
    public string TenLop { get; set; } = "";
    public int NganhId { get; set; }
    public string MaNganh { get; set; } = "";
    public string TenNganh { get; set; } = "";
    public int SoSinhVien { get; set; }
}
