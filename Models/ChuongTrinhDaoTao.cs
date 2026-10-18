namespace BTL_StudentMGMT.Models;

public class ChuongTrinhDaoTao
{
    public int Id { get; set; }
    public string? MaChuongTrinh { get; set; }
    public string TenChuongTrinh { get; set; } = "";
    public string LoaiChuongTrinh { get; set; } = "";
    public string KhoaQuanLy { get; set; } = "";
    public string? KhoaChuyenMon { get; set; }
    public string? MaNganhGoc { get; set; }
    public string? HinhThucLienKet { get; set; }
    public int? ApDungTuKhoa { get; set; }
    public bool TichHopCuNhanKySu { get; set; }
    public string? GhiChu { get; set; }
    public int? NganhId { get; set; }
    public Nganh? Nganh { get; set; }
}
