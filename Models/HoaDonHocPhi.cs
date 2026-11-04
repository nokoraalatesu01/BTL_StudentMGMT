using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_StudentMGMT.Models;

public class HoaDonHocPhi
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Mã khoản thu / Số hóa đơn")]
    public string SoHoaDon { get; set; } = string.Empty;

    [Required]
    public int SinhVienId { get; set; }
    public virtual SinhVien SinhVien { get; set; } = null!;

    [Required]
    public int HocKyId { get; set; }
    public virtual HocKy HocKy { get; set; } = null!;

    [Required]
    [StringLength(200)]
    [Display(Name = "Tên khoản thu")]
    public string TenKhoanThu { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Số tiền phải đóng")]
    public decimal SoTien { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Đã thanh toán")]
    public decimal DaThanhToan { get; set; } = 0;

    [NotMapped]
    [Display(Name = "Còn phải đóng")]
    public decimal ConPhaiDong => Math.Max(0, SoTien - DaThanhToan);

    public DateTime NgayLap { get; set; } = DateTime.UtcNow;

    public DateOnly? HanThanhToan { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "CHUA_THANH_TOAN"; // CHUA_THANH_TOAN, THANH_TOAN_MOT_PHAN, DA_THANH_TOAN, QUA_HAN

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }

    public int? DotThuId { get; set; }
    public virtual DotThuHocPhi? DotThu { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
