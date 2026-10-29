using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class GiangVien
{
    public int GiangVienId { get; set; }

    public string MaGiangVien { get; set; } = null!;

    public string Ho { get; set; } = null!;

    public string? TenDem { get; set; }

    public string Ten { get; set; } = null!;

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public int DonViId { get; set; }

    public string? IdentityUserId { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual DonVi DonVi { get; set; } = null!;

    public virtual ICollection<LopHanhChinh> LopHanhChinhs { get; set; } = new List<LopHanhChinh>();

    public virtual ICollection<LopHocPhan> LopHocPhans { get; set; } = new List<LopHocPhan>();

    public virtual TaiKhoanGiangVien? TaiKhoanGiangVien { get; set; }
}
