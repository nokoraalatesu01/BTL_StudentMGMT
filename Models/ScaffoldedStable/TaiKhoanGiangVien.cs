using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class TaiKhoanGiangVien
{
    public int TaiKhoanId { get; set; }

    public int GiangVienId { get; set; }

    public virtual GiangVien GiangVien { get; set; } = null!;

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;
}
