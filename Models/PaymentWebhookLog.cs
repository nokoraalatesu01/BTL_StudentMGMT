using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class PaymentWebhookLog
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = "VIETQR_VCB";

    [StringLength(100)]
    public string? EventId { get; set; }

    [Required]
    public string Payload { get; set; } = string.Empty;

    public bool SignatureValid { get; set; }

    [Required]
    [StringLength(30)]
    public string ProcessingStatus { get; set; } = "RECEIVED"; // RECEIVED, PROCESSED, IGNORED, FAILED, MISMATCH

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public string? ErrorMessage { get; set; }
}
