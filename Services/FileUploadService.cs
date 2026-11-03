namespace BTL_StudentMGMT.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string?> UploadAvatarAsync(IFormFile? file, string? oldAvatarPath = null)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            _logger.LogWarning("File tải lên vượt quá giới hạn 5MB: {Size} bytes", file.Length);
            return null;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            _logger.LogWarning("Đuôi file không được hỗ trợ: {Ext}", ext);
            return null;
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var fileStream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        // Xóa avatar cũ nếu tồn tại trong thư mục uploads/avatars
        if (!string.IsNullOrEmpty(oldAvatarPath))
        {
            DeleteFile(oldAvatarPath);
        }

        return $"/uploads/avatars/{uniqueFileName}";
    }

    public async Task<string?> UploadPaymentProofAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            _logger.LogWarning("Ảnh biên lai vượt quá giới hạn 5MB: {Size} bytes", file.Length);
            return null;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            _logger.LogWarning("Đuôi ảnh biên lai không được hỗ trợ: {Ext}", ext);
            return null;
        }

        var proofsFolder = Path.Combine(_env.ContentRootPath, "App_Data", "PaymentProofs");
        Directory.CreateDirectory(proofsFolder);

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(proofsFolder, uniqueFileName);
        await using var fileStream = new FileStream(physicalPath, FileMode.CreateNew);
        await file.CopyToAsync(fileStream);
        return uniqueFileName;
    }

    public Task<Stream?> OpenPaymentProofAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
        {
            return Task.FromResult<Stream?>(null);
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return Task.FromResult<Stream?>(null);
        }

        var physicalPath = Path.Combine(_env.ContentRootPath, "App_Data", "PaymentProofs", fileName);
        Stream? stream = File.Exists(physicalPath)
            ? new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)
            : null;
        return Task.FromResult(stream);
    }

    public void DeletePaymentProof(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
        {
            return;
        }

        var physicalPath = Path.Combine(_env.ContentRootPath, "App_Data", "PaymentProofs", fileName);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            var cleanedPath = relativePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, cleanedPath);

            // Chỉ xóa file nếu nằm trong wwwroot/uploads/
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            if (fullPath.StartsWith(uploadsDir, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa file cũ: {Path}", relativePath);
        }
    }
}
