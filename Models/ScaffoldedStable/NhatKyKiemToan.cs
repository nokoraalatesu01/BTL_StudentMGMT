using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class NhatKyKiemToan
{
    public long NhatKyId { get; set; }

    public string? UserId { get; set; }

    public string HanhDong { get; set; } = null!;

    public string TenBang { get; set; } = null!;

    public string? KhoaBanGhi { get; set; }

    public DateTime ThoiDiem { get; set; }

    public string? IpAddress { get; set; }

    public string? CorrelationId { get; set; }

    public string? DuLieuCu { get; set; }

    public string? DuLieuMoi { get; set; }
}
