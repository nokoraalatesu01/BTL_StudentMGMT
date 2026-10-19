namespace BTL_StudentMGMT.Models;

public class ThongKeLop
{
    public string TenLop { get; set; } = "";
    public int SoSinhVien { get; set; }
}

public class AdminDashboardViewModel
{
    public int TongSinhVien { get; set; }
    public int TongGiangVien { get; set; }
    public int TongNganh { get; set; }
    public int TongLop { get; set; }
    public int TongMonHoc { get; set; }
    public int TongTaiKhoan { get; set; }
    public int SinhVienChuaCoLop { get; set; }
    public int SinhVienChuaCoTaiKhoan { get; set; }
    public int GiangVienChuaCoTaiKhoan { get; set; }
    public List<ThongKeLop> ThongKeTheoLop { get; set; } = new();
}
