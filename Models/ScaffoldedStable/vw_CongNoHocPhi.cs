using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class vw_CongNoHocPhi
{
    public long HoaDonId { get; set; }

    public string SoHoaDon { get; set; } = null!;

    public string MaSinhVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string TenHocKy { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public decimal? TongTien { get; set; }

    public decimal? DaThanhToan { get; set; }

    public decimal? ConNo { get; set; }
}
