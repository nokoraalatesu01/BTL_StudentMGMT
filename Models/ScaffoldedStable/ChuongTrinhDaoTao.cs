using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class ChuongTrinhDaoTao
{
    public int ChuongTrinhId { get; set; }

    public string MaChuongTrinh { get; set; } = null!;

    public string TenChuongTrinh { get; set; } = null!;

    public int NganhId { get; set; }

    public string TrinhDo { get; set; } = null!;

    public short TongTinChiYeuCau { get; set; }

    public decimal? SoNamDaoTao { get; set; }

    public DateOnly HieuLucTu { get; set; }

    public DateOnly? HieuLucDen { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<BieuPhiHocPhi> BieuPhiHocPhis { get; set; } = new List<BieuPhiHocPhi>();

    public virtual ICollection<ChuongTrinhMonHoc> ChuongTrinhMonHocs { get; set; } = new List<ChuongTrinhMonHoc>();

    public virtual ICollection<LopHanhChinh> LopHanhChinhs { get; set; } = new List<LopHanhChinh>();

    public virtual Nganh Nganh { get; set; } = null!;
}
