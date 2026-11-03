namespace BTL_StudentMGMT.Models;

public class DiemDanhItemViewModel
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? TenLopHanhChinh { get; set; }
    public string TrangThai { get; set; } = "Có mặt"; // "Có mặt", "Vắng", "Có phép"
    public string? GhiChu { get; set; }
}

public class DiemDanhFormViewModel
{
    // Hỗ trợ điểm danh theo Lớp học phần (chuẩn tín chỉ)
    public int? LopHocPhanId { get; set; }
    public string? TenLopHocPhan { get; set; }

    // Giữ lại để tương thích ngược dữ liệu cũ
    public int LopId { get; set; }
    public int MonHocId { get; set; }
    public DateTime NgayDiemDanh { get; set; } = DateTime.Today;
    public List<DiemDanhItemViewModel> DanhSachSinhVien { get; set; } = new();
}
