using System.Threading.Tasks;
using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Services;

public interface IVietQrService
{
    Task<VietQrConfig> GetConfigAsync();
    Task UpdateConfigAsync(VietQrConfig config);
    string GenerateVietQrUrl(decimal amount, string content, VietQrConfig? config = null);
    bool VerifyWebhookSignature(string payload, string? signature, string secret);
}
