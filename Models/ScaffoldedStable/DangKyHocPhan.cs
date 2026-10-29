using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class DangKyHocPhan
{
    public long DangKyId { get; set; }

    public int SinhVienId { get; set; }

    public int LopHocPhanId { get; set; }

    public DateTime NgayDangKy { get; set; }

    public string TrangThai { get; set; } = null!;

    public byte LanHoc { get; set; }

    public string? GhiChu { get; set; }

    public byte[] RowVer { get; set; } = null!;

    public virtual ICollection<DiemThanhPhan> DiemThanhPhans { get; set; } = new List<DiemThanhPhan>();

    public virtual KetQuaHocTap? KetQuaHocTap { get; set; }

    public virtual ICollection<LichSuChinhSuaDiem> LichSuChinhSuaDiems { get; set; } = new List<LichSuChinhSuaDiem>();

    public virtual LopHocPhan LopHocPhan { get; set; } = null!;

    public virtual SinhVien SinhVien { get; set; } = null!;
}
