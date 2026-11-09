using System.Collections.Generic;
using System.Linq;

namespace BTL_StudentMGMT.Models;

public class NhapDiemExcelViewModel
{
    public int? KhoaId { get; set; }
    public int? LopId { get; set; }
    public int? MonHocId { get; set; }
    public int? LopHocPhanId { get; set; }
    public int? HocKyId { get; set; }

    public string? TenKhoa { get; set; }
    public string? TenLop { get; set; }
    public string? TenMonHoc { get; set; }
    public string? MaMon { get; set; }
    public int? SoTinChi { get; set; }
    public string? TenLopHocPhan { get; set; }

    // Thống kê nhanh
    public int TongSoSinhVien => DanhSachSinhVien?.Count ?? 0;
    public int SoLuongDaNhapDu => DanhSachSinhVien?.Count(s => s.DiemChuyenCan.HasValue && s.DiemGiuaKy.HasValue && s.DiemCuoiKy.HasValue) ?? 0;
    public int SoLuongDat => DanhSachSinhVien?.Count(s => s.DiemTongKet.HasValue && s.DiemTongKet.Value >= 4.0) ?? 0;

    public List<BangDiemExcelRowModel> DanhSachSinhVien { get; set; } = new();
}

public class BangDiemExcelRowModel
{
    public int SinhVienId { get; set; }
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string? NgaySinhStr { get; set; }
    public string? TenLop { get; set; }

    public int? DiemId { get; set; }
    public int? LopHocPhanId { get; set; }

    public double? DiemChuyenCan { get; set; }
    public double? DiemGiuaKy { get; set; }
    public double? DiemCuoiKy { get; set; }
    public double? DiemCuoiKyLan2 { get; set; }

    public double? DiemTongKet { get; set; }
    public string? DiemChu { get; set; }
    public double? DiemHe4 { get; set; }
    public string? TrangThai { get; set; }
    public string? GhiChu { get; set; }

    public bool IsDuDiem => DiemChuyenCan.HasValue && DiemGiuaKy.HasValue && DiemCuoiKy.HasValue;
    public bool IsDat => DiemTongKet.HasValue && DiemTongKet.Value >= 4.0;
}

