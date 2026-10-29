using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class BuoiDiemDanh
{
    public long BuoiDiemDanhId { get; set; }

    public int LopHocPhanId { get; set; }

    public DateTime ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public string? NoiDung { get; set; }

    public string? NguoiTao { get; set; }

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual LopHocPhan LopHocPhan { get; set; } = null!;
}
