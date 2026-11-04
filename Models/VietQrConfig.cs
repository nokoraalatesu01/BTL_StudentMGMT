using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class VietQrConfig
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "Mã định danh ngân hàng (BIN)")]
    public string BankId { get; set; } = "970436"; // 970436 - Vietcombank

    [Required]
    [StringLength(100)]
    [Display(Name = "Tên ngân hàng")]
    public string BankName { get; set; } = "Vietcombank (Ngân hàng TMCP Ngoại thương Việt Nam)";

    [Required]
    [StringLength(50)]
    [Display(Name = "Số tài khoản thụ hưởng")]
    public string AccountNumber { get; set; } = "1043572004"; // Cấu hình linh hoạt

    [Required]
    [StringLength(100)]
    [Display(Name = "Tên chủ tài khoản")]
    public string AccountHolderName { get; set; } = "TRUONG DAI HOC GIAO THONG VAN TAI";

    [Required]
    [StringLength(20)]
    [Display(Name = "Mẫu giao diện VietQR")]
    public string Template { get; set; } = "compact2"; // compact, compact2, qr_only, print

    [Required]
    [StringLength(255)]
    [Display(Name = "VietQR API Base URL")]
    public string ApiBaseUrl { get; set; } = "https://api.vietqr.io";

    [StringLength(100)]
    [Display(Name = "Client ID")]
    public string? ClientId { get; set; }

    [StringLength(100)]
    [Display(Name = "API Key")]
    public string? ApiKey { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Webhook Secret Key")]
    public string WebhookSecret { get; set; } = "vcb_vietqr_secret_utc_2026";

    [Required]
    [StringLength(20)]
    [Display(Name = "Môi trường tích hợp")]
    public string Environment { get; set; } = "Sandbox"; // "Sandbox" | "Production"

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
