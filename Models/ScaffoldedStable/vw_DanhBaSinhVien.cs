using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class vw_DanhBaSinhVien
{
    public int SinhVienId { get; set; }

    public string MaSinhVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? EmailTruong { get; set; }

    public string? SoDienThoai { get; set; }

    public string TrangThai { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public string TenLop { get; set; } = null!;

    public string MaNganh { get; set; } = null!;

    public string TenNganh { get; set; } = null!;

    public string TenChuongTrinh { get; set; } = null!;
}
