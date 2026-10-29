using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class LopHanhChinh
{
    public int LopId { get; set; }

    public string MaLop { get; set; } = null!;

    public string TenLop { get; set; } = null!;

    public int NganhId { get; set; }

    public int ChuongTrinhId { get; set; }

    public int KhoaTuyenSinhId { get; set; }

    public int? CoVanGiangVienId { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ChuongTrinhDaoTao ChuongTrinh { get; set; } = null!;

    public virtual GiangVien? CoVanGiangVien { get; set; }

    public virtual KhoaTuyenSinh KhoaTuyenSinh { get; set; } = null!;

    public virtual Nganh Nganh { get; set; } = null!;

    public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}
