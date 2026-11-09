using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;
    public FixedTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;
    public override DateTimeOffset GetUtcNow() => _utcNow;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}

public class AttendanceServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private (ApplicationDbContext db, LopHocPhan lhp, HocKy hk, ClaimsPrincipal gvUser, ClaimsPrincipal adminUser, AttendanceService service) CreateSetup(DateTimeOffset now)
    {
        var db = CreateInMemoryDbContext();

        var hocKy = new HocKy
        {
            HocKyId = 1,
            MaHocKy = "HK1_2627",
            TenHocKy = "Học kỳ 1 (2026 - 2027)",
            NgayBatDau = new DateOnly(2026, 8, 1),
            NgayKetThuc = new DateOnly(2026, 12, 31),
            TrangThai = "DANG_MO"
        };
        db.HocKies.Add(hocKy);

        var monHoc = new MonHoc
        {
            Id = 1,
            MaMon = "INT301",
            TenMon = "Lập trình Web",
            SoTinChi = 3
        };
        db.MonHocs.Add(monHoc);

        var lhp = new LopHocPhan
        {
            Id = 10,
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Lập trình Web",
            MonHocId = 1,
            MonHoc = monHoc,
            Thu = "Thứ 2",
            HocKyId = 1,
            HocKy = hocKy,
            TrangThai = "OPEN"
        };
        db.LopHocPhans.Add(lhp);

        var sv1 = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var sv2 = new SinhVien { Id = 2, MaSinhVien = "SV02", HoTen = "Tran Thi B" };
        db.SinhViens.AddRange(sv1, sv2);

        db.DangKyHocPhans.AddRange(
            new DangKyHocPhan { SinhVienId = 1, LopHocPhanId = 10, TrangThai = EnrollmentStatus.Enrolled },
            new DangKyHocPhan { SinhVienId = 2, LopHocPhanId = 10, TrangThai = EnrollmentStatus.Enrolled }
        );
        db.SaveChanges();

        var timeProvider = new FixedTimeProvider(now);
        var securityMock = new Mock<IGiangVienSecurityService>();
        securityMock.Setup(s => s.CanAccessLopHocPhanAsync(It.IsAny<ClaimsPrincipal>(), 10)).ReturnsAsync(true);

        var options = Options.Create(new AttendanceOptions { EditWindowDays = 7 });
        var service = new AttendanceService(db, securityMock.Object, options, timeProvider, NullLogger<AttendanceService>.Instance);

        var gvUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "gv01"),
            new Claim(ClaimTypes.Role, "GiangVien")
        }, "mock"));

        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        return (db, lhp, hocKy, gvUser, adminUser, service);
    }

    // 1. Điều kiện ngày: Sai thứ trong tuần
    [Fact]
    public async Task ValidateSessionDate_WrongDayOfWeek_ShouldFail()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero); // Chủ nhật
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            // 2026-11-03 là Thứ 3, nhưng lớp học Thứ 2
            var result = await service.ValidateSessionDateAsync(10, new DateTime(2026, 11, 3), gvUser);
            Assert.False(result.IsValid);
            Assert.Contains("không khớp với lịch học", result.ErrorMessage);
        }
    }

    // 2. Điều kiện ngày: Ngày trong tương lai
    [Fact]
    public async Task ValidateSessionDate_FutureDate_ShouldFail()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            // 2026-11-09 là Thứ 2, sau ngày hiện tại 2026-11-08
            var result = await service.ValidateSessionDateAsync(10, new DateTime(2026, 11, 9), gvUser);
            Assert.False(result.IsValid);
            Assert.Contains("tương lai", result.ErrorMessage);
        }
    }

    // 3. Điều kiện ngày: Ngoài thời gian học kỳ (trước ngày bắt đầu kỳ học)
    [Fact]
    public async Task ValidateSessionDate_OutsideSemester_ShouldFail()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            // 2026-07-27 là Thứ 2 nhưng trước 2026-08-01
            var result = await service.ValidateSessionDateAsync(10, new DateTime(2026, 7, 27), gvUser);
            Assert.False(result.IsValid);
            Assert.Contains("ngoài thời gian học kỳ", result.ErrorMessage);
        }
    }

    // 4. Điều kiện ngày: Quá cửa sổ sửa (EditWindowDays=7) đối với giảng viên
    [Fact]
    public async Task ValidateSessionDate_OutsideEditWindow_ForLecturer_ShouldFail()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            // 2026-10-26 là Thứ 2 (13 ngày trước ngày hiện tại 2026-11-08)
            var result = await service.ValidateSessionDateAsync(10, new DateTime(2026, 10, 26), gvUser);
            Assert.False(result.IsValid);
            Assert.True(result.IsOutsideEditWindow);
            Assert.Contains("thời hạn cho phép chỉnh sửa", result.ErrorMessage);
        }
    }

    // 5. Admin bỏ qua cửa sổ sửa
    [Fact]
    public async Task ValidateSessionDate_OutsideEditWindow_ForAdmin_ShouldSucceed()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, _, adminUser, service) = CreateSetup(now);
        using (db)
        {
            // 2026-10-26 quá 7 ngày nhưng Admin được phép
            var result = await service.ValidateSessionDateAsync(10, new DateTime(2026, 10, 26), adminUser);
            Assert.True(result.IsValid);
        }
    }

    // 6. Buổi học hợp lệ trong cửa sổ sửa
    [Fact]
    public async Task ValidateSessionDate_ValidDateWithinWindow_ShouldSucceed()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            // 2026-11-02 là Thứ 2 (cách 6 ngày <= 7 ngày)
            var result = await service.ValidateSessionDateAsync(10, new DateTime(2026, 11, 2), gvUser);
            Assert.True(result.IsValid);
        }
    }

    // 7. Lớp học phần không tồn tại
    [Fact]
    public async Task ValidateSessionDate_ClassNotFound_ShouldFail()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            var result = await service.ValidateSessionDateAsync(999, new DateTime(2026, 11, 2), gvUser);
            Assert.False(result.IsValid);
            Assert.Contains("Không tìm thấy", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
    }

    // 8. Giảng viên khác bị từ chối
    [Fact]
    public async Task SaveDiemDanh_UnauthorizedLecturer_ShouldBeRejected()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, _, _, _) = CreateSetup(now);
        using (db)
        {
            var unauthorizedMock = new Mock<IGiangVienSecurityService>();
            unauthorizedMock.Setup(s => s.CanAccessLopHocPhanAsync(It.IsAny<ClaimsPrincipal>(), 10)).ReturnsAsync(false);

            var service = new AttendanceService(db, unauthorizedMock.Object, Options.Create(new AttendanceOptions()), new FixedTimeProvider(now), NullLogger<AttendanceService>.Instance);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "GiangVien") }));

            var model = new DiemDanhFormViewModel
            {
                LopHocPhanId = 10,
                NgayDiemDanh = new DateTime(2026, 11, 2),
                DanhSachSinhVien = new List<DiemDanhItemViewModel>
                {
                    new DiemDanhItemViewModel { SinhVienId = 1, TrangThai = AttendanceStatus.Present }
                }
            };

            var result = await service.SaveDiemDanhAsync(model, user);
            Assert.False(result.Success);
            Assert.Contains("không có quyền", result.Message);
        }
    }

    // 9. Thiếu dòng (sinh viên chưa chọn trạng thái) thì không lưu gì
    [Fact]
    public async Task SaveDiemDanh_MissingStudentStatus_ShouldNotSaveAnything()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            var model = new DiemDanhFormViewModel
            {
                LopHocPhanId = 10,
                NgayDiemDanh = new DateTime(2026, 11, 2),
                DanhSachSinhVien = new List<DiemDanhItemViewModel>
                {
                    new DiemDanhItemViewModel { SinhVienId = 1, TrangThai = AttendanceStatus.Present },
                    new DiemDanhItemViewModel { SinhVienId = 2, TrangThai = null } // Thiếu dòng
                }
            };

            var result = await service.SaveDiemDanhAsync(model, gvUser);
            Assert.False(result.Success);
            Assert.Contains("chưa có trạng thái", result.Message);

            // Kiểm tra DB không lưu bản ghi dở dang nào
            Assert.Equal(0, await db.DiemDanhs.CountAsync(d => d.LopHocPhanId == 10));
        }
    }

    // 10. SinhVienId không ENROLLED bị từ chối
    [Fact]
    public async Task SaveDiemDanh_StudentNotEnrolled_ShouldBeRejected()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            // Sinh viên 99 không ENROLLED trong lớp 10
            var model = new DiemDanhFormViewModel
            {
                LopHocPhanId = 10,
                NgayDiemDanh = new DateTime(2026, 11, 2),
                DanhSachSinhVien = new List<DiemDanhItemViewModel>
                {
                    new DiemDanhItemViewModel { SinhVienId = 1, TrangThai = AttendanceStatus.Present },
                    new DiemDanhItemViewModel { SinhVienId = 99, TrangThai = AttendanceStatus.Present }
                }
            };

            var result = await service.SaveDiemDanhAsync(model, gvUser);
            Assert.False(result.Success);
            Assert.Contains("không thuộc danh sách", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    // 11. Lưu hai lần không trùng (Upsert)
    [Fact]
    public async Task SaveDiemDanh_SavingTwice_ShouldUpdateExistingAndNotDuplicate()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            var date = new DateTime(2026, 11, 2);

            // Lần 1: SV1 có mặt, SV2 vắng
            var model1 = new DiemDanhFormViewModel
            {
                LopHocPhanId = 10,
                NgayDiemDanh = date,
                DanhSachSinhVien = new List<DiemDanhItemViewModel>
                {
                    new DiemDanhItemViewModel { SinhVienId = 1, TrangThai = AttendanceStatus.Present },
                    new DiemDanhItemViewModel { SinhVienId = 2, TrangThai = AttendanceStatus.Absent }
                }
            };
            var res1 = await service.SaveDiemDanhAsync(model1, gvUser);
            Assert.True(res1.Success);
            Assert.Equal(2, await db.DiemDanhs.CountAsync(d => d.LopHocPhanId == 10 && d.NgayDiemDanh.Date == date.Date));

            // Lần 2: Cập nhật lại cùng ngày (SV2 đổi thành Có phép)
            var model2 = new DiemDanhFormViewModel
            {
                LopHocPhanId = 10,
                NgayDiemDanh = date,
                DanhSachSinhVien = new List<DiemDanhItemViewModel>
                {
                    new DiemDanhItemViewModel { SinhVienId = 1, TrangThai = AttendanceStatus.Present },
                    new DiemDanhItemViewModel { SinhVienId = 2, TrangThai = AttendanceStatus.Excused }
                }
            };
            var res2 = await service.SaveDiemDanhAsync(model2, gvUser);
            Assert.True(res2.Success);

            // Tổng số bản ghi vẫn là 2, không nhân đôi
            var records = await db.DiemDanhs.Where(d => d.LopHocPhanId == 10 && d.NgayDiemDanh.Date == date.Date).ToListAsync();
            Assert.Equal(2, records.Count);
            var sv2Record = records.First(r => r.SinhVienId == 2);
            Assert.Equal(AttendanceStatus.Excused, sv2Record.TrangThai);
        }
    }

    // 12. Lưu hợp lệ toàn bộ sinh viên thành công
    [Fact]
    public async Task SaveDiemDanh_ValidSubmission_ShouldPersistAllRecords()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            var date = new DateTime(2026, 11, 2);
            var model = new DiemDanhFormViewModel
            {
                LopHocPhanId = 10,
                NgayDiemDanh = date,
                DanhSachSinhVien = new List<DiemDanhItemViewModel>
                {
                    new DiemDanhItemViewModel { SinhVienId = 1, TrangThai = AttendanceStatus.Present, GhiChu = "Đầy đủ" },
                    new DiemDanhItemViewModel { SinhVienId = 2, TrangThai = AttendanceStatus.Late, GhiChu = "Muộn 10p" }
                }
            };

            var res = await service.SaveDiemDanhAsync(model, gvUser);
            Assert.True(res.Success);
            Assert.Equal(2, await db.DiemDanhs.CountAsync(d => d.LopHocPhanId == 10));
        }
    }

    // 13. Xóa điểm danh khi hủy đăng ký
    [Fact]
    public async Task DeleteAttendanceForStudent_ShouldRemoveAllRecordsForClass()
    {
        using var db = CreateInMemoryDbContext();

        db.DiemDanhs.AddRange(
            new DiemDanh { SinhVienId = 1, LopHocPhanId = 10, NgayDiemDanh = new DateTime(2026, 10, 5), TrangThai = AttendanceStatus.Present },
            new DiemDanh { SinhVienId = 1, LopHocPhanId = 10, NgayDiemDanh = new DateTime(2026, 10, 12), TrangThai = AttendanceStatus.Absent },
            new DiemDanh { SinhVienId = 1, LopHocPhanId = 20, NgayDiemDanh = new DateTime(2026, 10, 6), TrangThai = AttendanceStatus.Present },
            new DiemDanh { SinhVienId = 2, LopHocPhanId = 10, NgayDiemDanh = new DateTime(2026, 10, 5), TrangThai = AttendanceStatus.Present }
        );
        await db.SaveChangesAsync();

        var service = new AttendanceService(
            db,
            Mock.Of<IGiangVienSecurityService>(),
            Options.Create(new AttendanceOptions()),
            TimeProvider.System,
            NullLogger<AttendanceService>.Instance);

        var deletedCount = await service.DeleteAttendanceForStudentAsync(10, 1);
        Assert.Equal(2, deletedCount);

        var remainingForSv1InLhp10 = await db.DiemDanhs.CountAsync(d => d.SinhVienId == 1 && d.LopHocPhanId == 10);
        Assert.Equal(0, remainingForSv1InLhp10);

        var remainingOtherClass = await db.DiemDanhs.CountAsync(d => d.SinhVienId == 1 && d.LopHocPhanId == 20);
        Assert.Equal(1, remainingOtherClass);
    }

    // 14. Lấy danh sách các ngày học hợp lệ của một lớp học phần trong kỳ học
    [Fact]
    public async Task GetValidSessionDates_ShouldReturnAllSessionDatesInSemester()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, _, _, service) = CreateSetup(now);
        using (db)
        {
            var dates = await service.GetValidSessionDatesAsync(10);
            Assert.NotEmpty(dates);
            // Tất cả các ngày trả về phải đúng là Thứ 2 (DayOfWeek.Monday)
            Assert.All(dates, d => Assert.Equal(DayOfWeek.Monday, d.DayOfWeek));
        }
    }

    // 15. Giảng viên không có quyền truy cập gọi form -> trả về null và ghi log vi phạm
    [Fact]
    public async Task GetDiemDanhForm_UnauthorizedLecturer_ShouldReturnNull()
    {
        using var db = CreateInMemoryDbContext();
        var secMock = new Mock<IGiangVienSecurityService>();
        secMock.Setup(s => s.CanAccessLopHocPhanAsync(It.IsAny<ClaimsPrincipal>(), 10)).ReturnsAsync(false);

        var service = new AttendanceService(
            db,
            secMock.Object,
            Options.Create(new AttendanceOptions()),
            TimeProvider.System,
            NullLogger<AttendanceService>.Instance);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "GiangVien") }));

        var form = await service.GetDiemDanhFormAsync(10, new DateTime(2026, 11, 2), user);
        Assert.Null(form);
    }

    // 16. Nạp form điểm danh thành công cho giảng viên hợp lệ
    [Fact]
    public async Task GetDiemDanhForm_ValidLecturer_ShouldReturnFormWithStudents()
    {
        var now = new DateTimeOffset(2026, 11, 8, 10, 0, 0, TimeSpan.Zero);
        var (db, _, _, gvUser, _, service) = CreateSetup(now);
        using (db)
        {
            var form = await service.GetDiemDanhFormAsync(10, new DateTime(2026, 11, 2), gvUser);
            Assert.NotNull(form);
            Assert.Equal(10, form.LopHocPhanId);
            Assert.Equal(2, form.DanhSachSinhVien.Count);
            Assert.Equal("SV01", form.DanhSachSinhVien[0].MaSinhVien);
            Assert.Equal("SV02", form.DanhSachSinhVien[1].MaSinhVien);
        }
    }
}
