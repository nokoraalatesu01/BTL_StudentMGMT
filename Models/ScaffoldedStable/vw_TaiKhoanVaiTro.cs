using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class vw_TaiKhoanVaiTro
{
    public int TaiKhoanId { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string HoTenHienThi { get; set; } = null!;

    public string? Email { get; set; }

    public string TrangThaiTaiKhoan { get; set; } = null!;

    public string? MaVaiTro { get; set; }

    public string? TenVaiTro { get; set; }

    public string? MaSinhVien { get; set; }

    public string? MaGiangVien { get; set; }

    public string? MaNhanVienPGV { get; set; }
}
