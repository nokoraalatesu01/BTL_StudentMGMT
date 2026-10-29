using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.TempScaffold;

public partial class SinhVien
{
    public int SinhVienId { get; set; }

    public string MaSinhVien { get; set; } = null!;

    public string Ho { get; set; } = null!;

    public string? TenDem { get; set; }

    public string Ten { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? EmailTruong { get; set; }

    public string? EmailCaNhan { get; set; }

    public string? SoDienThoai { get; set; }

    public byte[]? CCCDHash { get; set; }

    public string? DiaChiThuongTru { get; set; }

    public string? DiaChiLienHe { get; set; }

    public int LopId { get; set; }

    public DateOnly NgayNhapHoc { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? IdentityUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVer { get; set; } = null!;
}
