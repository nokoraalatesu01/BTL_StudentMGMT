using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class ChuongTrinhMonHoc
{
    public int ChuongTrinhId { get; set; }

    public int MonHocId { get; set; }

    public byte? HocKyDuKien { get; set; }

    public bool BatBuoc { get; set; }

    public string? GhiChu { get; set; }

    public virtual ChuongTrinhDaoTao ChuongTrinh { get; set; } = null!;

    public virtual MonHoc MonHoc { get; set; } = null!;
}
