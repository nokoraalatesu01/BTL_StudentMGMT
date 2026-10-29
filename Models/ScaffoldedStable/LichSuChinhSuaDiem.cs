using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class LichSuChinhSuaDiem
{
    public long LichSuDiemId { get; set; }

    public long DangKyId { get; set; }

    public decimal? DiemCu { get; set; }

    public decimal? DiemMoi { get; set; }

    public string LyDo { get; set; } = null!;

    public string NguoiThayDoi { get; set; } = null!;

    public DateTime ThoiDiem { get; set; }

    public virtual DangKyHocPhan DangKy { get; set; } = null!;
}
