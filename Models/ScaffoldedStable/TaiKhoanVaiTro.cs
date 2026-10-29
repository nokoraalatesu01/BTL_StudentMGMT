using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class TaiKhoanVaiTro
{
    public int TaiKhoanId { get; set; }

    public byte VaiTroId { get; set; }

    public DateTime NgayGan { get; set; }

    public int? NguoiGanTaiKhoanId { get; set; }

    public virtual TaiKhoan? NguoiGanTaiKhoan { get; set; }

    public virtual TaiKhoan TaiKhoan { get; set; } = null!;

    public virtual VaiTro VaiTro { get; set; } = null!;
}
