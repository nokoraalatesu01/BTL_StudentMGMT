using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class HocPhanTienQuyet
{
    public int MonHocId { get; set; }

    public int MonTienQuyetId { get; set; }

    public string LoaiDieuKien { get; set; } = null!;

    public virtual MonHoc MonHoc { get; set; } = null!;

    public virtual MonHoc MonTienQuyet { get; set; } = null!;
}
