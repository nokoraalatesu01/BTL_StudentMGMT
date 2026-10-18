namespace BTL_StudentMGMT.Models;

public class Nganh
{
    public int Id { get; set; }
    public string MaNganh { get; set; } = "";
    public string TenNganh { get; set; } = "";
    public List<Lop> Lops { get; set; } = new();
}
