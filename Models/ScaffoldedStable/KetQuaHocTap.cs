using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class KetQuaHocTap
{
    public long DangKyId { get; set; }

    public decimal? DiemTongKet { get; set; }

    public string? DiemChu { get; set; }

    public decimal? DiemHe4 { get; set; }

    public bool? Dat { get; set; }

    public byte TinChiTinhTichLuy { get; set; }

    public DateTime? NgayChot { get; set; }

    public string? NguoiChot { get; set; }

    public int PhienBan { get; set; }

    public virtual DangKyHocPhan DangKy { get; set; } = null!;
}
