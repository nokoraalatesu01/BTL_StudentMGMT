# BTL_StudentMGMT – Hệ thống quản lý sinh viên

Ứng dụng quản lý sinh viên được xây dựng bằng **ASP.NET Core MVC**, **Entity Framework Core**, **ASP.NET Core Identity** và **SQL Server**, chạy trên **.NET 10**.

## 1. Chức năng chính

Dự án tổ chức các nghiệp vụ quản lý đào tạo và sinh viên, gồm:

- Quản lý ngành và lớp.
- Quản lý sinh viên.
- Quản lý giảng viên.
- Quản lý môn học.
- Quản lý điểm.
- Quản lý điểm danh.
- Quản lý lịch giảng dạy.
- Đăng nhập, đăng xuất và phân quyền bằng ASP.NET Core Identity.

Một số bảng đã có trong mô hình dữ liệu để phục vụ mở rộng nghiệp vụ, nhưng chưa nhất thiết đã có màn hình quản lý tương ứng. Ví dụ: năm học, học kỳ, khóa tuyển sinh, phòng học, thành phần điểm, điểm thành phần, kết quả học tập và chương trình học bổng.

## 2. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Nền tảng | .NET 10 |
| Backend | ASP.NET Core MVC |
| ORM | Entity Framework Core 10 |
| Cơ sở dữ liệu | Microsoft SQL Server |
| Xác thực và phân quyền | ASP.NET Core Identity |
| Giao diện | Razor Views, HTML, CSS, JavaScript |

## 3. Yêu cầu môi trường

Cần cài đặt:

- .NET 10 SDK.
- SQL Server (ví dụ SQL Server Express).
- SQL Server Management Studio (SSMS) để quản lý database, nếu cần.
- Visual Studio 2022 phiên bản hỗ trợ .NET 10 hoặc Visual Studio Code.

Kiểm tra SDK:

```bash
dotnet --version
```

## 4. Cấu hình cơ sở dữ liệu

Mở file `appsettings.json`, kiểm tra chuỗi kết nối:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS06;Database=BTL_StudentMGMTDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Thay `Server=localhost\\SQLEXPRESS06` bằng tên SQL Server instance đang cài trên máy của bạn.

Ví dụ:

- SQL Server Express: `localhost\\SQLEXPRESS`
- SQL Server mặc định: `localhost`
- LocalDB: `(localdb)\\MSSQLLocalDB`

Ứng dụng hiện sử dụng Windows Authentication (`Trusted_Connection=True`). Tài khoản Windows chạy ứng dụng cần có quyền truy cập SQL Server và quyền tạo/cập nhật database.

## 5. Cách chạy dự án

Mở Terminal tại thư mục chứa file `BTL_StudentMGMT.csproj`, sau đó chạy:

```bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

Nếu máy chưa có lệnh `dotnet ef`, cài công cụ:

```bash
dotnet tool install --global dotnet-ef
```

Nếu đã cài, cập nhật công cụ:

```bash
dotnet tool update --global dotnet-ef
```

Sau khi chạy ứng dụng, mở địa chỉ HTTP/HTTPS được hiển thị trong Terminal. Trang đăng nhập là:

```text
/Account/Login
```

> Lưu ý: ứng dụng có gọi `DbSeeder.SeedAsync()` khi khởi động để tạo dữ liệu mẫu. Seeder hiện có đoạn gọi `MigrateAsync()`. Nếu database không được tạo/cập nhật như mong đợi, hãy kiểm tra log và kết nối SQL Server; không nên bỏ qua lỗi migration.

## 6. Tài khoản và mật khẩu demo

Các tài khoản dưới đây được khai báo trong `Data/DbSeeder.cs`. Seeder tạo tài khoản nếu username chưa tồn tại.

| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Quản trị viên (Admin) | `admin` | `Admin@123` |
| Giảng viên | `gv01` | `GiangVien@123` |
| Sinh viên | `sv01` | `SinhVien@123` |

### Phân quyền sau khi đăng nhập

| Vai trò | Điều hướng sau đăng nhập |
|---|---|
| Admin | Trang quản trị (`Admin/Index`) |
| Giảng viên | Trang giảng viên (`GiangVien/Index`) |
| Sinh viên | Trang sinh viên (`SinhVien/Index`) |

**Lưu ý về tài khoản demo:**

- Đây là tài khoản phục vụ chạy thử và trình diễn đồ án, không dùng cho môi trường production.
- Tài khoản chỉ được tạo khi Seeder chạy thành công và username tương ứng chưa tồn tại trong database.
- Nếu tài khoản đã tồn tại, Seeder không tự động đặt lại mật khẩu. Vì vậy, mật khẩu trong bảng trên có thể không còn đúng nếu tài khoản đã được thay đổi trước đó.
- Nếu đăng nhập sai 5 lần liên tiếp, Identity được cấu hình khóa đăng nhập trong 5 phút.
- Nên đổi mật khẩu và loại bỏ tài khoản demo trước khi triển khai thực tế.

## 7. Dữ liệu mẫu

`Data/DbSeeder.cs` hiện khai báo dữ liệu mẫu cho một số danh mục và nghiệp vụ, gồm:

- Các ngành và lớp mẫu: CNTT, DTVT, QTKD, NNA.
- Một tài khoản giảng viên và hồ sơ giảng viên mẫu.
- Một tài khoản sinh viên và danh sách sinh viên mẫu.
- Một số môn học.
- Dữ liệu điểm.
- Lịch giảng dạy và dữ liệu điểm danh mẫu.

Dữ liệu được thêm theo điều kiện kiểm tra bảng/danh mục tương ứng đang rỗng. Nếu database đã có dữ liệu, Seeder không nhất thiết bổ sung hoặc cập nhật toàn bộ dữ liệu mẫu.

## 8. Cấu trúc dữ liệu chính

Các bảng nghiệp vụ chính được sử dụng trong hệ thống:

- `Nganhs`
- `Lops`
- `SinhViens`
- `GiangViens`
- `MonHocs`
- `Diems`
- `ChuongTrinhDaoTaos`
- `DiemDanhs`
- `LichGiangDays`

Các bảng Identity gồm những bảng có tiền tố `AspNet`, dùng để quản lý người dùng, vai trò, đăng nhập và quyền truy cập.

Ngoài ra, mô hình dữ liệu còn có các bảng phục vụ mở rộng như `NamHoc`, `HocKy`, `KhoaTuyenSinh`, `PhongHoc`, `LichHoc`, `ThanhPhanDiem`, `DiemThanhPhan`, `KetQuaHocTap`, `ChuongTrinhHocBong` và `DonVi`. Trạng thái triển khai chức năng của từng bảng cần được đối chiếu với source code hiện tại.

## 9. Migration

Các migration nằm trong thư mục:

```text
Migrations/
```

Xem danh sách migration:

```bash
dotnet ef migrations list
```

Cập nhật database theo migration:

```bash
dotnet ef database update
```

Kiểm tra model có thay đổi chưa được tạo migration hay không:

```bash
dotnet ef migrations has-pending-model-changes
```

Không xóa hoặc chỉnh sửa migration đã được áp dụng trên database đang có dữ liệu nếu chưa sao lưu và đánh giá ảnh hưởng. Khi thay đổi model, hãy tạo migration mới sau khi kiểm tra kỹ:

```bash
dotnet ef migrations add TenMigration
dotnet ef database update
```

## 10. Lưu ý bảo mật và triển khai

- Không đưa mật khẩu thật, connection string chứa thông tin nhạy cảm hoặc khóa bí mật lên Git.
- Không sử dụng tài khoản demo trong môi trường production.
- Nên chuyển cấu hình nhạy cảm sang User Secrets hoặc biến môi trường.
- Sao lưu database trước khi chạy migration trên database có dữ liệu quan trọng.
- Kiểm tra quyền truy cập theo vai trò ở cả Controller/Action, không chỉ ẩn mục menu trên giao diện.
- Bật HTTPS khi triển khai.
- Thay đổi mật khẩu demo và rà soát chính sách khóa tài khoản trước khi đưa vào sử dụng thật.

## 11. Xử lý lỗi thường gặp

### Không kết nối được SQL Server

- Kiểm tra SQL Server Service đã chạy chưa.
- Kiểm tra đúng tên instance trong `DefaultConnection`.
- Kiểm tra Windows Authentication và quyền của tài khoản Windows.
- Kiểm tra database có được tạo/cập nhật thành công không.

### Không đăng nhập được bằng tài khoản demo

- Đảm bảo Seeder đã chạy thành công.
- Kiểm tra username đã tồn tại từ trước hay chưa.
- Nếu tài khoản tồn tại, mật khẩu có thể đã được thay đổi; Seeder không tự đặt lại mật khẩu cho tài khoản cũ.
- Kiểm tra tài khoản có bị khóa do nhập sai mật khẩu nhiều lần không.

### Lỗi migration

Chạy:

```bash
dotnet ef migrations list
dotnet ef database update
```

Đọc thông báo lỗi đầy đủ trong Terminal và kiểm tra `__EFMigrationsHistory`. Không nên bỏ qua lỗi migration hoặc tự ý xóa bảng để chạy lại.

---

**Tên dự án:** BTL_StudentMGMT  
**Mục đích:** Bài tập lớn xây dựng hệ thống quản lý sinh viên.
