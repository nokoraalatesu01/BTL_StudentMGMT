using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.TempTestCapHocBong;

public partial class CapHocBong
{
    public long CapHocBongId { get; set; }

    public int HocBongId { get; set; }

    public int SinhVienId { get; set; }

    public decimal SoTien { get; set; }

    public DateOnly NgayQuyetDinh { get; set; }

    public string? SoQuyetDinh { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }
}
