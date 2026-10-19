namespace BTL_StudentMGMT.Models;

public class DiemDanhItemViewModel
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string TrangThai { get; set; } = "Có mặt"; // "Có mặt", "Vắng", "Có phép"
    public string? GhiChu { get; set; }
}

public class DiemDanhFormViewModel
{
    public int LopId { get; set; }
    public int MonHocId { get; set; }
    public DateTime NgayDiemDanh { get; set; } = DateTime.Today;
    public List<DiemDanhItemViewModel> DanhSachSinhVien { get; set; } = new();
}
