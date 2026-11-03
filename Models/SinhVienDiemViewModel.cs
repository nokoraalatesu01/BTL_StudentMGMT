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

    // Điểm trung bình chung tích lũy GPA (Hệ 10)
    // Chỉ tính các bản ghi SuDungTinhTichLuy = true (mỗi môn học chỉ tính 1 lần kết quả chuẩn)
    public double? Gpa
    {
        get
        {
            var validDiems = Diems.Where(d => d.SuDungTinhTichLuy).ToList();
            if (validDiems.Count == 0 && Diems.Count > 0)
            {
                validDiems = Diems
                    .GroupBy(d => d.MonHocId)
                    .Select(g => g.OrderByDescending(d => d.DiemTongKet).First())
                    .ToList();
            }

            if (validDiems.Count == 0) return null;

            double tongDiem = 0;
            double tongTinChi = 0;

            foreach (var diem in validDiems)
            {
                var tinChi = diem.MonHoc?.SoTinChi > 0 ? diem.MonHoc.SoTinChi : 1;
                tongDiem += diem.DiemTongKet * tinChi;
                tongTinChi += tinChi;
            }

            return tongTinChi == 0 ? null : Math.Round(tongDiem / tongTinChi, 2);
        }
    }

    // Điểm trung bình chung tích lũy GPA (Hệ 4)
    public double? GpaHe4
    {
        get
        {
            var validDiems = Diems.Where(d => d.SuDungTinhTichLuy).ToList();
            if (validDiems.Count == 0 && Diems.Count > 0)
            {
                validDiems = Diems
                    .GroupBy(d => d.MonHocId)
                    .Select(g => g.OrderByDescending(d => d.DiemTongKet).First())
                    .ToList();
            }

            if (validDiems.Count == 0) return null;

            double tongDiemHe4 = 0;
            double tongTinChi = 0;

            foreach (var diem in validDiems)
            {
                var tinChi = diem.MonHoc?.SoTinChi > 0 ? diem.MonHoc.SoTinChi : 1;
                tongDiemHe4 += diem.DiemHe4 * tinChi;
                tongTinChi += tinChi;
            }

            return tongTinChi == 0 ? null : Math.Round(tongDiemHe4 / tongTinChi, 2);
        }
    }

    public string XepLoai
    {
        get
        {
            var gpa = Gpa;
            if (gpa == null) return "Chưa có điểm";
            if (gpa >= 8.5) return "Giỏi";
            if (gpa >= 7.0) return "Khá";
            return "Trung bình";
        }
    }

    // Số môn học duy nhất đã đạt yêu cầu (>= 4.0)
    public int SoMonDaDat => Diems
        .Where(diem => diem.DiemTongKet >= 4.0)
        .Select(diem => diem.MonHocId)
        .Distinct()
        .Count();

    // Tổng số môn học duy nhất đã đăng ký học
    public int SoMon => Diems
        .Select(diem => diem.MonHocId)
        .Distinct()
        .Count();

    // Tổng số tín chỉ tích lũy (chỉ tính 1 lần cho mỗi môn học đạt)
    public int TongTinChiDaDat => Diems
        .Where(diem => diem.SuDungTinhTichLuy && diem.DiemTongKet >= 4.0)
        .Sum(diem => diem.MonHoc?.SoTinChi > 0 ? diem.MonHoc.SoTinChi : 1);

    // Danh sách nhóm lịch sử học tập theo môn học (phục vụ hiển thị chi tiết các lần học)
    public IEnumerable<IGrouping<int, Diem>> LichSuTheoMonHoc => Diems
        .OrderBy(d => d.MonHoc?.TenMon)
        .ThenBy(d => d.LanHoc)
        .ThenBy(d => d.LanThi)
        .GroupBy(d => d.MonHocId);
}
