namespace BTL_StudentMGMT.Helpers;

public static class DisplayHelper
{
    /// <summary>
    /// Hiển thị giá trị chuỗi hoặc giá trị thay thế nếu chuỗi rỗng / chỉ chứa khoảng trắng.
    /// Giúp tránh lỗi hiển thị khi trường chuỗi có giá trị mặc định là "" thay vì null.
    /// </summary>
    public static string DisplayOr(string? value, string fallback = "Chưa cập nhật")
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
