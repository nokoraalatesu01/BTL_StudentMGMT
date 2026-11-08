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

        var candidateServers = new List<string>();

        // 1. Ưu tiên cao nhất: Server được cấu hình trong appsettings.json
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

        // 2. Thêm các instance SQLEXPRESS phổ biến (từ SQLEXPRESS06 xuống SQLEXPRESS)
        var commonInstances = new[]
        {
            "SQLEXPRESS06", "SQLEXPRESS05", "SQLEXPRESS04",
            "SQLEXPRESS03", "SQLEXPRESS02", "SQLEXPRESS01", "SQLEXPRESS"
        };

        foreach (var inst in commonInstances)
        {
            candidateServers.Add(@$".\{inst}");
            candidateServers.Add(@$"{machineName}\{inst}");
            candidateServers.Add(@$"localhost\{inst}");
        }

        // 3. Thêm SQL Server mặc định (default instance)
        candidateServers.Add(".");
        candidateServers.Add("localhost");

        // 4. Cuối cùng mới đến LocalDB (dự phòng)
        candidateServers.Add(@"(localdb)\mssqllocaldb");

        var uniqueServers = candidateServers
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        string? firstRespondingServer = null;

        foreach (var server in uniqueServers)
        {
            // Bước 1: Thử kết nối TRỰC TIẾP vào database đích trước (nếu đã có DB và quyền truy cập tốt)
            var directBuilder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 2
            };

            try
            {
                using var directConn = new SqlConnection(directBuilder.ConnectionString);
                directConn.Open();

                // Kết nối thẳng vào DB thành công 100%
                Console.WriteLine($"[Database] Tự động kết nối trực tiếp database '{databaseName}' thành công trên: {server}");
                _cachedConnectionString = directBuilder.ConnectionString;
                return _cachedConnectionString;
            }
            catch
            {
                // Chưa kết nối trực tiếp được vào databaseName trên server này, kiểm tra master
            }

            // Bước 2: Kiểm tra server có hoạt động thông qua master không
            var masterBuilder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = "master",
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 1
            };

            try
            {
                using var masterConn = new SqlConnection(masterBuilder.ConnectionString);
                masterConn.Open();

                // Lưu lại server đầu tiên có master phản hồi để làm fallback
                firstRespondingServer ??= server;

                // Kiểm tra xem databaseName đã tồn tại trên server này chưa
                using var cmd = masterConn.CreateCommand();
                cmd.CommandText = "SELECT DB_ID(@dbName)";
                cmd.Parameters.Add(new SqlParameter("@dbName", databaseName));
                var dbId = cmd.ExecuteScalar();

                if (dbId != null && dbId != DBNull.Value)
                {
                    // Server này đã có databaseName sẵn!
                    var resultBuilder = new SqlConnectionStringBuilder
                    {
                        DataSource = server,
                        InitialCatalog = databaseName,
                        IntegratedSecurity = true,
                        TrustServerCertificate = true,
                        ConnectTimeout = 30
                    };

                    Console.WriteLine($"[Database] Tìm thấy database '{databaseName}' tồn tại trên SQL Server: {server}");
                    _cachedConnectionString = resultBuilder.ConnectionString;
                    return _cachedConnectionString;
                }
            }
            catch
            {
                // Thử ứng viên tiếp theo
            }
        }

        // Nếu không server nào có sẵn database, chọn server đầu tiên phản hồi master để EF Core tạo DB
        if (!string.IsNullOrEmpty(firstRespondingServer))
        {
            var fallbackBuilder = new SqlConnectionStringBuilder
            {
                DataSource = firstRespondingServer,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 30
            };

            Console.WriteLine($"[Database] Chọn SQL Server khả dụng ({firstRespondingServer}) để chuẩn bị khởi tạo database '{databaseName}'.");
            _cachedConnectionString = fallbackBuilder.ConnectionString;
            return _cachedConnectionString;
        }

        _cachedConnectionString = baseConnectionString ?? $@"Server=.\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";
        return _cachedConnectionString;
    }
}
