using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class HoSoSinhVien
{
    public long HoSoId { get; set; }

    public int SinhVienId { get; set; }

    public string LoaiHoSo { get; set; } = null!;

    public string TenTep { get; set; } = null!;

    public string UriLuuTru { get; set; } = null!;

    public string? MimeType { get; set; }

    public long? KichThuocBytes { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? NguoiDuyet { get; set; }

    public DateTime? NgayDuyet { get; set; }

    public string? GhiChu { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SinhVien SinhVien { get; set; } = null!;
}
