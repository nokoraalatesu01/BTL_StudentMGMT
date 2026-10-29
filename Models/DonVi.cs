using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models;

public partial class DonVi
{
    public int DonViId { get; set; }

    public string MaDonVi { get; set; } = null!;

    public string TenDonVi { get; set; } = null!;

    public string LoaiDonVi { get; set; } = null!;

    public int? DonViChaId { get; set; }

    public string? Email { get; set; }

    public string? DienThoai { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVer { get; set; } = null!;

    public virtual DonVi? DonViCha { get; set; }

    public virtual ICollection<GiangVien> GiangViens { get; set; } = new List<GiangVien>();

    public virtual ICollection<DonVi> InverseDonViCha { get; set; } = new List<DonVi>();

    public virtual ICollection<MonHoc> MonHocs { get; set; } = new List<MonHoc>();

    public virtual ICollection<ChuongTrinhDaoTao> ChuongTrinhQuanLy { get; set; } = new List<ChuongTrinhDaoTao>();

    public virtual ICollection<ChuongTrinhDaoTao> ChuongTrinhChuyenMon { get; set; } = new List<ChuongTrinhDaoTao>();

    public virtual ICollection<Nganh> Nganhs { get; set; } = new List<Nganh>();
}
