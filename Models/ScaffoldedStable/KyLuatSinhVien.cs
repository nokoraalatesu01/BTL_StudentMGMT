using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class KyLuatSinhVien
{
    public long KyLuatId { get; set; }

    public int SinhVienId { get; set; }

    public string LoaiViPham { get; set; } = null!;

    public string MoTa { get; set; } = null!;

    public string? SoQuyetDinh { get; set; }

    public DateOnly NgayQuyetDinh { get; set; }

    public DateOnly? NgayHetHieuLuc { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? NguoiLap { get; set; }

    public virtual SinhVien SinhVien { get; set; } = null!;
}
