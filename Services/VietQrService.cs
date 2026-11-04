using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BTL_StudentMGMT.Services;

public class VietQrService : IVietQrService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public VietQrService(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<VietQrConfig> GetConfigAsync()
    {
        var dbConfig = await _db.VietQrConfigs.FirstOrDefaultAsync();
        if (dbConfig != null)
        {
            return dbConfig;
        }

        // Lấy mặc định từ appsettings.json nếu DB chưa có
        var section = _config.GetSection("VietQrSettings");
        var defaultConfig = new VietQrConfig
        {
            BankId = section["BankId"] ?? "970436",
            BankName = section["BankName"] ?? "Vietcombank (Ngân hàng TMCP Ngoại thương Việt Nam)",
            AccountNumber = section["AccountNumber"] ?? "1043572004",
            AccountHolderName = section["AccountHolderName"] ?? "NGUYEN HUU HOANH",
            Template = section["Template"] ?? "compact2",
            ApiBaseUrl = section["ApiBaseUrl"] ?? "https://api.vietqr.io",
            ClientId = section["ClientId"],
            ApiKey = section["ApiKey"],
            WebhookSecret = section["WebhookSecret"] ?? "vcb_vietqr_secret_utc_2026",
            Environment = section["Environment"] ?? "Sandbox",
            UpdatedAt = DateTime.UtcNow
        };

        _db.VietQrConfigs.Add(defaultConfig);
        await _db.SaveChangesAsync();
        return defaultConfig;
    }

    public async Task UpdateConfigAsync(VietQrConfig newConfig)
    {
        var existing = await _db.VietQrConfigs.FirstOrDefaultAsync();
        if (existing == null)
        {
            newConfig.UpdatedAt = DateTime.UtcNow;
            _db.VietQrConfigs.Add(newConfig);
        }
        else
        {
            existing.BankId = newConfig.BankId;
            existing.BankName = newConfig.BankName;
            existing.AccountNumber = newConfig.AccountNumber;
            existing.AccountHolderName = newConfig.AccountHolderName;
            existing.Template = newConfig.Template;
            existing.ApiBaseUrl = newConfig.ApiBaseUrl;
            existing.ClientId = newConfig.ClientId;
            existing.ApiKey = newConfig.ApiKey;
            existing.WebhookSecret = newConfig.WebhookSecret;
            existing.Environment = newConfig.Environment;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public string GenerateVietQrUrl(decimal amount, string content, VietQrConfig? config = null)
    {
        config ??= GetConfigAsync().GetAwaiter().GetResult();

        // Chuẩn URL QuickLink Napas247 - VietQR
        // https://img.vietqr.io/image/<BANK_ID>-<ACCOUNT_NO>-<TEMPLATE>.png?amount=<AMOUNT>&addInfo=<DESCRIPTION>&accountName=<ACCOUNT_NAME>
        var bankId = config.BankId.Trim();
        var accountNo = config.AccountNumber.Trim();
        var template = string.IsNullOrWhiteSpace(config.Template) ? "compact2" : config.Template.Trim();
        var amountInt = (long)Math.Round(amount);
        var encodedInfo = Uri.EscapeDataString(content.Trim());
        var encodedName = Uri.EscapeDataString(config.AccountHolderName.Trim());

        return $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={amountInt}&addInfo={encodedInfo}&accountName={encodedName}";
    }

    public bool VerifyWebhookSignature(string payload, string? signature, string secret)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return string.Equals(computedSignature, signature.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
