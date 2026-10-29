using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class Nganh
{
    public int NganhId { get; set; }

    public string MaNganh { get; set; } = null!;

    public string TenNganh { get; set; } = null!;

    public int DonViId { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; } = new List<ChuongTrinhDaoTao>();

    public virtual DonVi DonVi { get; set; } = null!;

    public virtual ICollection<LopHanhChinh> LopHanhChinhs { get; set; } = new List<LopHanhChinh>();
}
