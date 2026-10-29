using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models;

public partial class NamHoc
{
    public int NamHocId { get; set; }

    public string MaNamHoc { get; set; } = null!;

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<HocKy> HocKies { get; set; } = new List<HocKy>();
    public virtual ICollection<ChuongTrinhHocBong> ChuongTrinhHocBongs { get; set; } = new List<ChuongTrinhHocBong>();
}
