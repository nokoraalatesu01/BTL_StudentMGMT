using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class LopHocPhan
{
    public int LopHocPhanId { get; set; }

    public string MaLopHocPhan { get; set; } = null!;

    public int MonHocId { get; set; }

    public int HocKyId { get; set; }

    public int GiangVienId { get; set; }

    public string Nhom { get; set; } = null!;

    public short SiSoToiDa { get; set; }

    public DateOnly? NgayMoDangKy { get; set; }

    public DateOnly? NgayDongDangKy { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<BuoiDiemDanh> BuoiDiemDanhs { get; set; } = new List<BuoiDiemDanh>();

    public virtual ICollection<DangKyHocPhan> DangKyHocPhans { get; set; } = new List<DangKyHocPhan>();

    public virtual GiangVien GiangVien { get; set; } = null!;

    public virtual HocKy HocKy { get; set; } = null!;

    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();

    public virtual MonHoc MonHoc { get; set; } = null!;

    public virtual ICollection<ThanhPhanDiem> ThanhPhanDiems { get; set; } = new List<ThanhPhanDiem>();
}
