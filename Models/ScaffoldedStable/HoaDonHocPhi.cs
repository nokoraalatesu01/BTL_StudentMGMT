using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class HoaDonHocPhi
{
    public long HoaDonId { get; set; }

    public string SoHoaDon { get; set; } = null!;

    public int SinhVienId { get; set; }

    public int HocKyId { get; set; }

    public DateTime NgayLap { get; set; }

    public DateOnly? HanThanhToan { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietHoaDonHocPhi> ChiTietHoaDonHocPhis { get; set; } = new List<ChiTietHoaDonHocPhi>();

    public virtual HocKy HocKy { get; set; } = null!;

    public virtual SinhVien SinhVien { get; set; } = null!;

    public virtual ICollection<ThanhToan> ThanhToans { get; set; } = new List<ThanhToan>();
}
