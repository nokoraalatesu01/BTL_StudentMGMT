using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class TaiKhoanNhanVienPGV
{
    public int TaiKhoanId { get; set; }

    public int NhanVienPGVId { get; set; }

    public virtual NhanVienPGV NhanVienPGV { get; set; } = null!;

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
