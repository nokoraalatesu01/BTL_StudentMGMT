using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

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

    public virtual ICollection<CapHocBong> CapHocBongs { get; set; } = new List<CapHocBong>();

    public virtual ICollection<DangKyHocPhan> DangKyHocPhans { get; set; } = new List<DangKyHocPhan>();

    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();

    public virtual ICollection<HoSoSinhVien> HoSoSinhViens { get; set; } = new List<HoSoSinhVien>();

    public virtual ICollection<HoaDonHocPhi> HoaDonHocPhis { get; set; } = new List<HoaDonHocPhi>();

    public virtual ICollection<KyLuatSinhVien> KyLuatSinhViens { get; set; } = new List<KyLuatSinhVien>();

    public virtual ICollection<LichSuTrangThaiSinhVien> LichSuTrangThaiSinhViens { get; set; } = new List<LichSuTrangThaiSinhVien>();

    public virtual LopHanhChinh Lop { get; set; } = null!;

    public virtual TaiKhoanSinhVien? TaiKhoanSinhVien { get; set; }
}
