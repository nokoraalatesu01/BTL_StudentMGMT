using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class ThanhToan
{
    public long ThanhToanId { get; set; }

    public long HoaDonId { get; set; }

    public string? MaGiaoDich { get; set; }

    public decimal SoTien { get; set; }

    public DateTime ThoiGianThanhToan { get; set; }

    public string PhuongThuc { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public string? NguonThanhToan { get; set; }

    public string? IdempotencyKey { get; set; }

    public string? GhiChu { get; set; }

    public virtual HoaDonHocPhi HoaDon { get; set; } = null!;
}
