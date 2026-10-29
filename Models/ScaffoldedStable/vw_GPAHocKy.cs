using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class vw_GPAHocKy
{
    public int SinhVienId { get; set; }

    public string MaSinhVien { get; set; } = null!;

    public int HocKyId { get; set; }

    public string TenHocKy { get; set; } = null!;

    public decimal? GPAHe4 { get; set; }

    public decimal? DiemTBHe10 { get; set; }

    public int? TinChiDat { get; set; }
}
