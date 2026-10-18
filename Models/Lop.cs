namespace BTL_StudentMGMT.Models;

public class Lop
{
    public int Id { get; set; }
    public string TenLop { get; set; } = "";
    public int NganhId { get; set; }
    public Nganh? Nganh { get; set; }
    public List<SinhVien> SinhViens { get; set; } = new();
}
