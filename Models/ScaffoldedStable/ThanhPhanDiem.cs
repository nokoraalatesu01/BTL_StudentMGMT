using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class ThanhPhanDiem
{
    public int ThanhPhanDiemId { get; set; }

    public int LopHocPhanId { get; set; }

    public string MaThanhPhan { get; set; } = null!;

    public string TenThanhPhan { get; set; } = null!;

    public decimal TrongSo { get; set; }

    public decimal DiemToiDa { get; set; }

    public bool BatBuoc { get; set; }

    public virtual ICollection<DiemThanhPhan> DiemThanhPhans { get; set; } = new List<DiemThanhPhan>();

    public virtual LopHocPhan LopHocPhan { get; set; } = null!;
}
