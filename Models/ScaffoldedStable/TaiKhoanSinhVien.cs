using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class TaiKhoanSinhVien
{
    public int TaiKhoanId { get; set; }

    public int SinhVienId { get; set; }

    public virtual SinhVien SinhVien { get; set; } = null!;

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
