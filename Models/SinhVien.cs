using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_StudentMGMT.Models;

public class SinhVien
{
    public int Id { get; set; }
    public string MaSinhVien { get; set; } = "";
    public string HoTen { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public int? LopId { get; set; }
    public Lop? Lop { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public string? NienKhoa { get; set; }
    public string? TrangThai { get; set; }
    public string? HinhAnh { get; set; }
    public List<Diem> Diems { get; set; } = new();

    // Bổ sung navigation từ schema mới (không ảnh hưởng code cũ)
    public virtual ICollection<DiemDanh> DiemDanhs { get; set; } = new List<DiemDanh>();
    public virtual ICollection<DangKyHocPhan> DangKyHocPhans { get; set; } = new List<DangKyHocPhan>();

    public static IReadOnlyList<string> TrangThaiOptions { get; } =
    [
        "Đang học",
        "Tốt nghiệp",
        "Bảo lưu",
        "Đình chỉ"
    ];

    [NotMapped]
    public double? Gpa
    {
        get
        {
            // Chỉ tính trên các bản ghi được đánh dấu tính tích lũy (SuDungTinhTichLuy == true)
            // Mỗi môn học chỉ tính đúng 1 lần kết quả tích lũy chuẩn
            var validDiems = Diems.Where(d => d.SuDungTinhTichLuy).ToList();
            if (validDiems.Count == 0 && Diems.Count > 0)
            {
                // Fallback: Group by MonHocId lấy điểm cao nhất
                validDiems = Diems
                    .GroupBy(d => d.MonHocId)
                    .Select(g => g.OrderByDescending(d => d.DiemTongKet).First())
                    .ToList();
            }

            if (validDiems.Count == 0)
            {
                return null;
            }

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

    [NotMapped]
    public int TongTinChiTichLuy
    {
        get
        {
            var validDiems = Diems.Where(d => d.SuDungTinhTichLuy && d.DiemTongKet >= 4.0).ToList();
            if (validDiems.Count == 0 && Diems.Count > 0)
            {
                validDiems = Diems
                    .Where(d => d.DiemTongKet >= 4.0)
                    .GroupBy(d => d.MonHocId)
                    .Select(g => g.OrderByDescending(d => d.DiemTongKet).First())
                    .ToList();
            }
            return validDiems.Sum(d => d.MonHoc?.SoTinChi > 0 ? d.MonHoc.SoTinChi : 1);
        }
    }

    [NotMapped]
    public int SoMonDaDat
    {
        get
        {
            // Đếm số môn học duy nhất đã đạt (>= 4.0)
            return Diems
                .Where(d => d.DiemTongKet >= 4.0)
                .Select(d => d.MonHocId)
                .Distinct()
                .Count();
        }
    }

    [NotMapped]
    public int TongSoMonHoc
    {
        get
        {
            // Đếm số môn học duy nhất đã học
            return Diems
                .Select(d => d.MonHocId)
                .Distinct()
                .Count();
        }
    }

    [NotMapped]
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
}
