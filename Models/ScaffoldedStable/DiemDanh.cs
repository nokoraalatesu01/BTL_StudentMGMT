using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class DiemDanh
{
    public long BuoiDiemDanhId { get; set; }

    public int SinhVienId { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public string? NguoiCapNhat { get; set; }

    public DateTime CapNhatLuc { get; set; }

    public virtual BuoiDiemDanh BuoiDiemDanh { get; set; } = null!;

    public virtual SinhVien SinhVien { get; set; } = null!;
}
