namespace BTL_StudentMGMT.Models;

public class DiemDanh
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }

    // Liên kết với Lớp học phần (chuẩn tín chỉ)
    public int? LopHocPhanId { get; set; }
    public LopHocPhan? LopHocPhan { get; set; }

    // Giữ lại để tương thích ngược với dữ liệu cũ (Lớp hành chính)
    public int? LopId { get; set; }
    public Lop? Lop { get; set; }

    public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }

    public DateTime NgayDiemDanh { get; set; } = DateTime.Today;
    public string TrangThai { get; set; } = AttendanceStatus.Present;
    public string? GhiChu { get; set; }
}
