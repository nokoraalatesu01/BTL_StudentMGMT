# Hướng dẫn kiểm tra bằng tay: Chống vượt sĩ số (Anti-Oversell) trên SQL Server

## 1. Lý do cần kiểm tra thủ công trên SQL Server thật
Khóa dòng `WITH (UPDLOCK, ROWLOCK)` trong `EnrollmentService.AcquireClassLockAsync` là tính năng đặc thù của Microsoft SQL Server:
```sql
SELECT 1 FROM dbo.LopHocPhan WITH (UPDLOCK, ROWLOCK) WHERE Id = @lopHocPhanId
```
Các nhà cung cấp cơ sở dữ liệu dùng trong kiểm thử tự động (như **SQLite In-Memory** hoặc **EF Core InMemory Database Provider**) không hỗ trợ cú pháp `WITH (UPDLOCK, ROWLOCK)` cũng như cơ chế transaction locking của SQL Server engine. Do đó, kiểm thử tự động trong project test sẽ bỏ qua bước này (`_db.Database.IsSqlServer() == false`), và kịch bản tranh chấp ghi danh đồng thời thực tế cần được kiểm chứng thủ công trên SQL Server thật theo các bước dưới đây.

---

## 2. Chuẩn bị dữ liệu kiểm thử
Mở **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio** kết nối đến cơ sở dữ liệu `QLSinhVienUTC` (hoặc CSDL local của dự án), chạy đoạn script sau:

```sql
-- 1. Tìm hoặc tạo một lớp học phần thử nghiệm có Sĩ số tối đa = 1
DECLARE @LhpId INT;
DECLARE @HocKyId INT = (SELECT TOP 1 HocKyId FROM dbo.HocKies WHERE TrangThai = 'DANG_MO');
DECLARE @MonHocId INT = (SELECT TOP 1 Id FROM dbo.MonHocs);

INSERT INTO dbo.LopHocPhan (MaLopHocPhan, TenLopHocPhan, MonHocId, HocKyId, Nhom, SiSoToiDa, TrangThai, Thu, TietBatDau, SoTiet, PhongHoc)
VALUES ('TEST.LOCK.01', N'Lớp kiểm tra khóa dòng', @MonHocId, @HocKyId, 99, 1, 'OPEN', N'Thứ 2', 1, 3, 'P101');

SET @LhpId = SCOPE_IDENTITY();

-- 2. Đảm bảo có 2 sinh viên khác nhau chưa đăng ký lớp này
SELECT TOP 2 Id, MaSinhVien, HoTen FROM dbo.SinhViens;
PRINT 'LopHocPhanId để test: ' + CAST(@LhpId AS VARCHAR(10));
```

Ghi lại `LopHocPhanId` (ví dụ `100`), và `Id` của Sinh viên 1 (`SV1`), Sinh viên 2 (`SV2`).

---

## 3. Kịch bản kiểm thử: 2 phiên đồng thời cùng ghi danh vào lớp còn 1 chỗ

### Cách 1: Sử dụng 2 trình duyệt Web khác nhau (hoặc 1 Regular + 1 Incognito)
1. Mở **Trình duyệt A** (Chrome): Đăng nhập tài khoản sinh viên `SV1`.
2. Mở **Trình duyệt B** (Chrome Ẩn danh / Firefox / Edge): Đăng nhập tài khoản sinh viên `SV2`.
3. Cả 2 sinh viên cùng truy cập màn hình: `https://localhost:<port>/SinhVien/DangKyHocPhan`.
4. Tìm lớp `TEST.LOCK.01` (Sĩ số hiện tại: `0/1`).
5. Chuẩn bị sẵn chuột trên nút **Đăng ký** của lớp ở cả 2 trình duyệt.
6. Bấm nút **Đăng ký** trên cả 2 trình duyệt cùng một thời điểm.

**Kết quả mong đợi:**
- **Một sinh viên thành công**: Nhận thông báo màu xanh `Đăng ký thành công lớp học phần 'Lớp kiểm tra khóa dòng'!`. Sĩ số lớp nâng lên `1/1`.
- **Sinh viên còn lại bị chặn an toàn**: Nhận thông báo lỗi màu đỏ `Lớp học phần đã đầy sĩ số (hết chỗ). Vui lòng chọn nhóm lớp khác.` (hoặc `Bạn đã đăng ký lớp học phần này rồi.` nếu cùng một sinh viên bấm đúp).
- **Tuyệt đối không xảy ra lỗi 500 (Internal Server Error)** và không bao giờ xuất hiện sĩ số `2/1`.

---

### Cách 2: Mô phỏng khóa dòng và kiểm tra độ trễ (Lock Delay Simulation)
Để quan sát rõ hàng đợi khóa dòng trong SQL Server:
1. Mở **Session 1 (SSMS Query Window 1)**:
   ```sql
   BEGIN TRANSACTION;
   -- Giữ khóa dòng lớp học phần
   SELECT 1 FROM dbo.LopHocPhan WITH (UPDLOCK, ROWLOCK) WHERE Id = <LhpId>;
   -- Giữ nguyên không COMMIT vội, để mô phỏng một transaction đang chạy
   ```
2. Trên Web, sinh viên bấm **Đăng ký**.
   - Quan sát thấy Request trên Web ở trạng thái chờ (Pending / Blocked) chứ không thất bại hay vượt sĩ số.
3. Trong **Session 2 (SSMS Query Window 2)**, kiểm tra trạng thái khóa:
   ```sql
   SELECT request_session_id, resource_type, request_mode, request_status 
   FROM sys.dm_tran_locks 
   WHERE resource_database_id = DB_ID();
   ```
   - Sẽ thấy khóa `UPDLOCK` trên resource Key/RID đang ở trạng thái `GRANT`, các request đăng ký đến sau ở trạng thái `WAIT`.
4. Quay lại **Session 1**, gõ:
   ```sql
   ROLLBACK TRANSACTION;
   ```
   - Ngay lập tức, request trên Web được nhả khóa và xử lý tiếp bình thường.

---

## 4. Dọn dẹp dữ liệu test
Sau khi kiểm tra xong, dọn dẹp dữ liệu thử nghiệm trong SSMS:
```sql
DELETE FROM dbo.DangKyHocPhan WHERE LopHocPhanId = <LhpId>;
DELETE FROM dbo.LopHocPhan WHERE Id = <LhpId>;
```
