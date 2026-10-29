using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class TaiKhoan
{
    public int TaiKhoanId { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string HoTenHienThi { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public bool BuocDoiMatKhau { get; set; }

    public short SoLanDangNhapSai { get; set; }

    public DateTime? KhoaDen { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVer { get; set; } = null!;

    public virtual TaiKhoanGiangVien? TaiKhoanGiangVien { get; set; }

    public virtual TaiKhoanNhanVienPGV? TaiKhoanNhanVienPGV { get; set; }

    public virtual TaiKhoanSinhVien? TaiKhoanSinhVien { get; set; }

    public virtual ICollection<TaiKhoanVaiTro> TaiKhoanVaiTroNguoiGanTaiKhoans { get; set; } = new List<TaiKhoanVaiTro>();

    public virtual ICollection<TaiKhoanVaiTro> TaiKhoanVaiTroTaiKhoans { get; set; } = new List<TaiKhoanVaiTro>();
}
