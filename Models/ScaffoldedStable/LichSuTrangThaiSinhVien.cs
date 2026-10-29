using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class LichSuTrangThaiSinhVien
{
    public long LichSuId { get; set; }

    public int SinhVienId { get; set; }

    public string? TrangThaiCu { get; set; }

    public string TrangThaiMoi { get; set; } = null!;

    public DateOnly NgayHieuLuc { get; set; }

    public string? LyDo { get; set; }

    public string? NguoiThayDoi { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual SinhVien SinhVien { get; set; } = null!;
}
