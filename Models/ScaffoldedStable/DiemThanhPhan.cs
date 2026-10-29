using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class DiemThanhPhan
{
    public long DangKyId { get; set; }

    public int ThanhPhanDiemId { get; set; }

    public decimal? Diem { get; set; }

    public string? NhanXet { get; set; }

    public string? NguoiNhap { get; set; }

    public DateTime? NgayNhap { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual DangKyHocPhan DangKy { get; set; } = null!;

    public virtual ThanhPhanDiem ThanhPhanDiem { get; set; } = null!;
}
