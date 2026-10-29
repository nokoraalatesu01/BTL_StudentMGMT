using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class PhongHoc
{
    public int PhongHocId { get; set; }

    public string MaPhong { get; set; } = null!;

    public string TenPhong { get; set; } = null!;

    public string? CoSo { get; set; }

    public short SucChua { get; set; }

    public string LoaiPhong { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();
}
