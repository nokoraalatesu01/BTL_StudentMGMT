using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class vw_DangKyHocPhanChiTiet
{
    public long DangKyId { get; set; }

    public string MaSinhVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string MaLopHocPhan { get; set; } = null!;

    public string MaMon { get; set; } = null!;

    public string TenMon { get; set; } = null!;

    public byte SoTinChi { get; set; }

    public string TenHocKy { get; set; } = null!;

    public string MaNamHoc { get; set; } = null!;

    public byte LanHoc { get; set; }

    public string TrangThaiDangKy { get; set; } = null!;

    public decimal? DiemTongKet { get; set; }

    public string? DiemChu { get; set; }

    public decimal? DiemHe4 { get; set; }

    public bool? Dat { get; set; }

    public byte? TinChiTinhTichLuy { get; set; }
}
