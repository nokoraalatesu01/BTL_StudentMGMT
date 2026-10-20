namespace BTL_StudentMGMT.Models;

public class DiemDanh
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }
    public int LopId { get; set; }
    public Lop? Lop { get; set; }
    public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }
    public DateTime NgayDiemDanh { get; set; } = DateTime.Today;
    public string TrangThai { get; set; } = "Có mặt"; // "Có mặt", "Vắng", "Có phép"
    public string? GhiChu { get; set; }
}
