namespace BTL_StudentMGMT.Services;

public interface IFileUploadService
{
    Task<string?> UploadAvatarAsync(IFormFile? file, string? oldAvatarPath = null);
    Task<string?> UploadPaymentProofAsync(IFormFile? file);
    Task<Stream?> OpenPaymentProofAsync(string fileName);
    void DeletePaymentProof(string fileName);
    void DeleteFile(string? relativePath);
}
