using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BTL_StudentMGMT.Models;

public class BankTransaction
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Mã giao dịch ngân hàng (FT)")]
    public string BankTransactionId { get; set; } = string.Empty; // Unique FT code

    [StringLength(50)]
    public string? TransactionId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Số tiền thực nhận")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(255)]
    [Display(Name = "Nội dung giao dịch")]
    public string TransactionContent { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    [Required]
    [StringLength(10)]
    public string Direction { get; set; } = "IN"; // "IN" (tiền vào)

    public long? PaymentId { get; set; }
    public virtual Payment? Payment { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public string? RawData { get; set; }
}
