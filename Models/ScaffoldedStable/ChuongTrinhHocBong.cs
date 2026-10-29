using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class ChuongTrinhHocBong
{
    public int HocBongId { get; set; }

    public string MaHocBong { get; set; } = null!;

    public string TenHocBong { get; set; } = null!;

    public int NamHocId { get; set; }

    public decimal? SoTienToiDa { get; set; }

    public string? TieuChi { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<CapHocBong> CapHocBongs { get; set; } = new List<CapHocBong>();

    public virtual NamHoc NamHoc { get; set; } = null!;
}
