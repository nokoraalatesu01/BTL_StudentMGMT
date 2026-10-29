using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class ChiTietHoaDonHocPhi
{
    public long ChiTietHoaDonId { get; set; }

    public long HoaDonId { get; set; }

    public string LoaiKhoanThu { get; set; } = null!;

    public string MoTa { get; set; } = null!;

    public decimal SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public decimal SoTienGiam { get; set; }

    public virtual HoaDonHocPhi HoaDon { get; set; } = null!;
}
