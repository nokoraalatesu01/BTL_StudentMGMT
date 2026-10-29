using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models;

public partial class LichHoc
{
    public int LichHocId { get; set; }

    public int LopHocPhanId { get; set; }

    public int PhongHocId { get; set; }

    public byte Thu { get; set; }

    public byte TietBatDau { get; set; }

    public byte SoTiet { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public TimeOnly? GioBatDau { get; set; }

    public TimeOnly? GioKetThuc { get; set; }

    public virtual PhongHoc PhongHoc { get; set; } = null!;
}
