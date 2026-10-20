namespace BTL_StudentMGMT.Models;

public class LichGiangDay
{
    public int Id { get; set; }
    public int? GiangVienId { get; set; }
    public GiangVien? GiangVien { get; set; }
    public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }
    public int LopId { get; set; }
    public Lop? Lop { get; set; }
    public string Thu { get; set; } = "Thứ 2"; // "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7"
    public int TietBatDau { get; set; } = 1; // 1 đến 10
    public int SoTiet { get; set; } = 3; // 2, 3, 4
    public string PhongHoc { get; set; } = "P.302-A1";
    public string? GhiChu { get; set; }
    public string HocKy { get; set; } = "Học kỳ 1 (2026 - 2027)";
}
