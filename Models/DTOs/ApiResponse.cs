namespace BTL_StudentMGMT.Models.DTOs;

/// <summary>
/// Chuẩn hóa định dạng phản hồi (Response Format) cho toàn bộ hệ thống RESTful Web API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "";
    public T? Data { get; set; }
    public int? TotalCount { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Thành công")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Paged(T data, int totalCount, int page, int pageSize, string message = "Lấy dữ liệu thành công")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default
        };
    }
}

