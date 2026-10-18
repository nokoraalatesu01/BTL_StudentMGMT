# BTL_StudentMGMT

Bài tập lớn quản lý sinh viên sử dụng ASP.NET Core MVC, Entity Framework Core và ASP.NET Core Identity trên .NET 10.

## Quản trị ngành và lớp

Đăng nhập bằng tài khoản Admin, sau đó mở các mục **Quản lý ngành** và **Quản lý lớp** ở thanh điều hướng.

- Danh mục 34 ngành theo niên giám cập nhật 8/2024 có thể được đồng bộ từ mục Quản lý ngành.
- Các chương trình IE0–IE10, CLC Quản lý xây dựng công trình giao thông được niên giám 2024 nêu thêm, và 3 chương trình liên kết quốc tế được lưu tách riêng với ngành cấp IV, do một ngành có thể có nhiều chương trình.
- Khoa chuyên môn được phân biệt với đơn vị quản lý chương trình; niên giám không ghi khoa của Kinh doanh quốc tế nên hiển thị rõ là chưa xác định.
- Danh sách lớp không có trong các tài liệu nguồn nên cần được tạo thủ công, gắn với ngành tương ứng.
- Không thể xóa ngành đang có lớp hoặc lớp đang có sinh viên; mã ngành và tên lớp trong cùng ngành phải duy nhất.

Sau khi cập nhật mã nguồn, áp dụng schema mới bằng:

```powershell
dotnet ef database update
```
