using Microsoft.Data.SqlClient;

namespace BTL_StudentMGMT.Data;

/// <summary>
/// Tự động dò tìm SQL Server đang hoạt động trên máy tính hiện tại
/// Giúp mọi thành viên kéo code về là chạy được ngay mà không cần sửa tên server.
/// </summary>
public static class DbConnectionHelper
{
    private static string? _cachedConnectionString;

    public static string ResolveConnectionString(string? baseConnectionString)
    {
        if (!string.IsNullOrEmpty(_cachedConnectionString))
        {
            return _cachedConnectionString;
        }

        string databaseName = "BTL_StudentMGMTDb";

        if (!string.IsNullOrWhiteSpace(baseConnectionString))
        {
            try
            {
                var originalBuilder = new SqlConnectionStringBuilder(baseConnectionString);
                if (!string.IsNullOrWhiteSpace(originalBuilder.InitialCatalog))
                {
                    databaseName = originalBuilder.InitialCatalog;
                }
            }
            catch { }
        }

        string machineName = Environment.MachineName;

        // Luôn ưu tiên SQL Server trên chính máy tính đang chạy (Environment.MachineName)
        // Dù người khác có push tên máy của họ lên Git thì máy bạn vẫn tự nhận diện máy bạn trước
        var candidateServers = new List<string>
        {
            $@"{machineName}\SQLEXPRESS",
            @".\SQLEXPRESS",
            @"localhost\SQLEXPRESS",
            ".",
            "localhost",
            @"(localdb)\mssqllocaldb"
        };

        // Nếu chuỗi cấu hình có server khác và chưa có trong danh sách thì thêm vào cuối để dự phòng
        if (!string.IsNullOrWhiteSpace(baseConnectionString))
        {
            try
            {
                var orig = new SqlConnectionStringBuilder(baseConnectionString);
                if (!string.IsNullOrWhiteSpace(orig.DataSource) && !candidateServers.Contains(orig.DataSource, StringComparer.OrdinalIgnoreCase))
                {
                    candidateServers.Add(orig.DataSource);
                }
            }
            catch { }
        }

        foreach (var server in candidateServers)
        {
            var testBuilder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = "master",
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 1
            };

            try
            {
                using var conn = new SqlConnection(testBuilder.ConnectionString);
                conn.Open();

                // Đã tìm thấy SQL Server đang phản hồi tốt
                var resultBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = server,
                    InitialCatalog = databaseName,
                    IntegratedSecurity = true,
                    TrustServerCertificate = true,
                    ConnectTimeout = 30
                };

                Console.WriteLine($"[Database] Tự động kết nối SQL Server thành công: {server}");
                _cachedConnectionString = resultBuilder.ConnectionString;
                return _cachedConnectionString;
            }
            catch
            {
                // Thử ứng viên tiếp theo
            }
        }

        _cachedConnectionString = baseConnectionString ?? $@"Server=.\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        return _cachedConnectionString;
    }
}
