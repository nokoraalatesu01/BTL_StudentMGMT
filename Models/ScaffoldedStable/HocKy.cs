using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class HocKy
{
    public int HocKyId { get; set; }

    public int NamHocId { get; set; }

    public string MaHocKy { get; set; } = null!;

    public string TenHocKy { get; set; } = null!;

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public DateOnly? HanDangKyTu { get; set; }

    public DateOnly? HanDangKyDen { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<HoaDonHocPhi> HoaDonHocPhis { get; set; } = new List<HoaDonHocPhi>();

    public virtual ICollection<LopHocPhan> LopHocPhans { get; set; } = new List<LopHocPhan>();

    public virtual NamHoc NamHoc { get; set; } = null!;
}
