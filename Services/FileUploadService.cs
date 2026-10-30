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
