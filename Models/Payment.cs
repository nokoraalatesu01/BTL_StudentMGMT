using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_StudentMGMT.Models;

public class Payment
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Mã giao dịch")]
    public string TransactionCode { get; set; } = string.Empty; // Mã nhận diện duy nhất (VD: HP20261104A1B2)

    [Required]
    public long HoaDonHocPhiId { get; set; }
    public virtual HoaDonHocPhi HoaDonHocPhi { get; set; } = null!;

    [Required]
    public int SinhVienId { get; set; }
    public virtual SinhVien SinhVien { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Số tiền thanh toán")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Nội dung chuyển khoản")]
    public string PaymentContent { get; set; } = string.Empty; // Chuẩn: HP {MaSV} {Code}

    [Required]
    [StringLength(50)]
    [Display(Name = "Phương thức")]
    public string PaymentMethod { get; set; } = "VIETQR_VCB"; // VIETQR_VCB, MANUAL_ADMIN

    [Required]
    [StringLength(30)]
    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "PENDING"; // PENDING, PAID, FAILED, EXPIRED, CANCELLED

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiredAt { get; set; }

    public DateTime? PaidAt { get; set; }

    [StringLength(100)]
    [Display(Name = "Mã giao dịch ngân hàng")]
    public string? BankTransactionId { get; set; } // FT code từ Vietcombank

    [StringLength(100)]
    [Display(Name = "Mã tham chiếu ngân hàng")]
    public string? BankReference { get; set; }

    [StringLength(500)]
    [Display(Name = "URL mã QR VietQR")]
    public string? QrUrl { get; set; }

    public string? RawWebhookData { get; set; }

    [Display(Name = "Xác nhận tự động")]
    public bool IsAutoVerified { get; set; } = false;

    public virtual BankTransaction? BankTransaction { get; set; }
}
