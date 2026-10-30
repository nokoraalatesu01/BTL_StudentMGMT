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
