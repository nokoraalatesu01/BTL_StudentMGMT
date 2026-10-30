namespace BTL_StudentMGMT.Models;

public class SinhVienDiemViewModel
{
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string? TenLop { get; set; }
    public string? TenNganh { get; set; }
    public string? NienKhoa { get; set; }
    public string? TrangThai { get; set; }
    public string? HinhAnh { get; set; }
    public List<Diem> Diems { get; set; } = new();

    public double? Gpa
    {
        get
        {
            if (Diems.Count == 0)
            {
                return null;
            }

            double tongDiem = 0;
            double tongTinChi = 0;

            foreach (var diem in Diems)
            {
                var tinChi = diem.MonHoc?.SoTinChi > 0 ? diem.MonHoc.SoTinChi : 1;
                tongDiem += diem.DiemTongKet * tinChi;
                tongTinChi += tinChi;
            }

            return tongTinChi == 0 ? null : Math.Round(tongDiem / tongTinChi, 2);
        }
    }

    public string XepLoai
    {
        get
        {
            var gpa = Gpa;
            if (gpa == null)
            {
                return "Chưa có điểm";
            }

            if (gpa >= 8.5) return "Giỏi";
            if (gpa >= 7.0) return "Khá";
            return "Trung bình";
        }
    }

    public int SoMonDaDat => Diems.Count(diem => diem.DiemTongKet >= 4.0);
    public int SoMon => Diems.Count;
    public int TongTinChiDaDat => Diems
        .Where(diem => diem.DiemTongKet >= 4.0)
        .Sum(diem => diem.MonHoc?.SoTinChi > 0 ? diem.MonHoc.SoTinChi : 1);
}
