using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models.ScaffoldedStable;

public partial class MonHoc
{
    public int MonHocId { get; set; }

    public string MaMon { get; set; } = null!;

    public string TenMon { get; set; } = null!;

    public byte SoTinChi { get; set; }

    public short SoTietLyThuyet { get; set; }

    public short SoTietThucHanh { get; set; }

    public int? DonViPhuTrachId { get; set; }

    public string LoaiMon { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<ChuongTrinhMonHoc> ChuongTrinhMonHocs { get; set; } = new List<ChuongTrinhMonHoc>();

    public virtual DonVi? DonViPhuTrach { get; set; }

    public virtual ICollection<HocPhanTienQuyet> HocPhanTienQuyetMonHocs { get; set; } = new List<HocPhanTienQuyet>();

    public virtual ICollection<HocPhanTienQuyet> HocPhanTienQuyetMonTienQuyets { get; set; } = new List<HocPhanTienQuyet>();

    public virtual ICollection<LopHocPhan> LopHocPhans { get; set; } = new List<LopHocPhan>();
}
