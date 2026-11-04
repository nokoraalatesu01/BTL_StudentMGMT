using System.IO;
using System.Text;
using System.Threading.Tasks;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTL_StudentMGMT.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentWebhookController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Endpoint nhận Webhook từ VietQR / Vietcombank thông báo biến động số dư
    /// </summary>
    [HttpPost("webhook/vietqr")]
    public async Task<IActionResult> ReceiveVietQrWebhook([FromBody] VietQrWebhookPayload payload)
    {
        string rawBody = string.Empty;
        Request.EnableBuffering();
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            Request.Body.Position = 0;
            rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
        }

        string? signatureHeader = Request.Headers["X-Webhook-Secret"].ToString();
        if (string.IsNullOrWhiteSpace(signatureHeader))
        {
            signatureHeader = Request.Headers["X-Signature"].ToString();
        }

        var (success, message) = await _paymentService.ProcessWebhookAsync(payload, rawBody, signatureHeader);

        if (success)
        {
            return Ok(new { success = true, message });
        }

        return BadRequest(new { success = false, message });
    }

    /// <summary>
    /// API tra cứu trạng thái giao dịch phục vụ Auto-Polling trên UI sinh viên
    /// </summary>
    [HttpGet("check-status/{transactionCode}")]
    public async Task<IActionResult> CheckStatus(string transactionCode)
    {
        var result = await _paymentService.CheckPaymentStatusAsync(transactionCode);
        return Ok(result);
    }

    /// <summary>
    /// API mô phỏng quét QR & chuyển khoản thành công (dành riêng cho môi trường Sandbox)
    /// </summary>
    [HttpPost("sandbox/simulate-transfer")]
    public async Task<IActionResult> SimulateTransfer([FromBody] SandboxTransferRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionCode))
        {
            return BadRequest(new { success = false, message = "Vui lòng cung cấp mã giao dịch cần mô phỏng." });
        }

        var (success, message) = await _paymentService.SimulateTransferAsync(request.TransactionCode, request.Amount);
        if (success)
        {
            return Ok(new { success = true, message });
        }

        return BadRequest(new { success = false, message });
    }
}

public class SandboxTransferRequest
{
    public string TransactionCode { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
}
