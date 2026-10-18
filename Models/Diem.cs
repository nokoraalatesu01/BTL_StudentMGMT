namespace BTL_StudentMGMT.Models;

public class Diem
{
    public int Id { get; set; }
    public int SinhVienId { get; set; }
    public SinhVien? SinhVien { get; set; }

    public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }

    public double DiemChuyenCan { get; set; }
    public double DiemGiuaKy { get; set; }
    public double DiemCuoiKy { get; set; }

    // Tính điểm tổng kết = CC * 10% + GK * 30% + CK * 60%
    public double DiemTongKet => Math.Round(DiemChuyenCan * 0.1 + DiemGiuaKy * 0.3 + DiemCuoiKy * 0.6, 2);

    // Xếp loại điểm chữ theo quy chế tín chỉ
    public string DiemChu
    {
        get
        {
            if (DiemTongKet >= 9.0) return "A+";
            if (DiemTongKet >= 8.5) return "A";
            if (DiemTongKet >= 8.0) return "B+";
            if (DiemTongKet >= 7.0) return "B";
            if (DiemTongKet >= 6.5) return "C+";
            if (DiemTongKet >= 5.5) return "C";
            if (DiemTongKet >= 5.0) return "D+";
            if (DiemTongKet >= 4.0) return "D";
            return "F";
        }
    }
}
