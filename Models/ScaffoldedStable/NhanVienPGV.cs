using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class NhanVienPGV
{
    public int NhanVienPGVId { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public int? DonViId { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual DonVi? DonVi { get; set; }

    public virtual TaiKhoanNhanVienPGV? TaiKhoanNhanVienPGV { get; set; }
}
