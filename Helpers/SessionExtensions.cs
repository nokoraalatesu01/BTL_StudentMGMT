using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace BTL_StudentMGMT.Helpers;

/// <summary>
/// Lớp tiện ích mở rộng cho Session, hỗ trợ lưu và đọc các đối tượng phức tạp (Object) qua định dạng JSON
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// Lưu bất kỳ object nào vào Session dưới dạng chuỗi JSON
    /// </summary>
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    /// <summary>
    /// Đọc và deserialize object từ chuỗi JSON trong Session
    /// </summary>
    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}

