namespace BTL_StudentMGMT.Services;

public interface IFileUploadService
{
    Task<string?> UploadAvatarAsync(IFormFile? file, string? oldAvatarPath = null);
    void DeleteFile(string? relativePath);
}
