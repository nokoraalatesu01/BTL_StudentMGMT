using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class ForceDuplicateOnSaveInterceptor : SaveChangesInterceptor
{
    private readonly SqliteConnection _connection;
    private readonly int _sinhVienId;
    private readonly int _lopHocPhanId;
    private bool _hasInjected;

    public ForceDuplicateOnSaveInterceptor(SqliteConnection connection, int sinhVienId, int lopHocPhanId)
    {
        _connection = connection;
        _sinhVienId = sinhVienId;
        _lopHocPhanId = lopHocPhanId;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (!_hasInjected && eventData.Context != null)
        {
            _hasInjected = true;
            using var cmd = _connection.CreateCommand();
            if (eventData.Context.Database.CurrentTransaction != null)
            {
                cmd.Transaction = eventData.Context.Database.CurrentTransaction.GetDbTransaction() as SqliteTransaction;
            }
            cmd.CommandText = $"INSERT INTO DangKyHocPhan (SinhVienId, LopHocPhanId, NgayDangKy, TrangThai, LanHoc, LoaiDangKy) " +
                             $"VALUES ({_sinhVienId}, {_lopHocPhanId}, '2026-11-09 00:00:00', 'ENROLLED', 1, 'HOC_LAN_DAU');";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

public class FailOnSaveInterceptor : SaveChangesInterceptor
{
    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated failure after SaveChanges but before transaction commit");
    }
}

public class EnrollmentServiceSqliteTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public EnrollmentServiceSqliteTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private ApplicationDbContext CreateContext(IInterceptor? interceptor = null)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection);
        if (interceptor != null)
        {
            builder.AddInterceptors(interceptor);
        }
        return new ApplicationDbContext(builder.Options);
    }

    private async Task SeedBasicDataAsync(ApplicationDbContext db)
    {
        db.Database.EnsureCreated();

        var namHoc = new NamHoc
        {
            NamHocId = 1,
            MaNamHoc = "NH2026",
            TrangThai = "DANG_HOAT_DONG",
            NgayBatDau = new DateOnly(2026, 9, 1),
            NgayKetThuc = new DateOnly(2027, 6, 30)
        };
        db.NamHocs.Add(namHoc);

        var hocKy = new HocKy
        {
            HocKyId = 1,
            NamHocId = 1,
            MaHocKy = "HK1",
            TenHocKy = "Học kỳ 1",
            TrangThai = "DANG_MO"
        };
        db.HocKies.Add(hocKy);

        var monHoc = new MonHoc
        {
            Id = 1,
            MaMon = "INT101",
            TenMon = "Lập trình C cơ bản",
            SoTinChi = 3
        };
        db.MonHocs.Add(monHoc);

        var sv = new SinhVien
        {
            Id = 1,
            MaSinhVien = "SV001",
            HoTen = "Nguyen Van A"
        };
        db.SinhViens.Add(sv);

        var lhp1 = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "INT101.01",
            TenLopHocPhan = "Lập trình C - Nhóm 1",
            MonHocId = 1,
            HocKyId = 1,
            SiSoToiDa = 10,
            TrangThai = "OPEN",
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 2
        };
        db.LopHocPhans.Add(lhp1);

        var lhp2 = new LopHocPhan
        {
            Id = 2,
            MaLopHocPhan = "INT101.02",
            TenLopHocPhan = "Lập trình C - Nhóm 2",
            MonHocId = 1,
            HocKyId = 1,
            SiSoToiDa = 10,
            TrangThai = "OPEN",
            Thu = "Thứ 4",
            TietBatDau = 1,
            SoTiet = 2
        };
        db.LopHocPhans.Add(lhp2);

        await db.SaveChangesAsync();
    }

    // 2.a. Unique index UQ_DK_SV_LHP thực sự chặn bản ghi trùng trên relational provider
    [Fact]
    public async Task Sqlite_UniqueIndex_UQ_DK_SV_LHP_PreventsDuplicateRecord()
    {
        using var db = CreateContext();
        await SeedBasicDataAsync(db);

        var dk1 = new DangKyHocPhan
        {
            SinhVienId = 1,
            LopHocPhanId = 1,
            NgayDangKy = DateTime.UtcNow,
            TrangThai = EnrollmentStatus.Enrolled
        };
        db.DangKyHocPhans.Add(dk1);
        await db.SaveChangesAsync();

        var dk2 = new DangKyHocPhan
        {
            SinhVienId = 1,
            LopHocPhanId = 1,
            NgayDangKy = DateTime.UtcNow,
            TrangThai = EnrollmentStatus.Enrolled
        };
        db.DangKyHocPhans.Add(dk2);

        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await db.SaveChangesAsync();
        });
    }

    // 2.b. Nhánh catch DbUpdateException trong EnrollAsync: ép vi phạm unique bằng Interceptor ngay trước SaveChanges
    [Fact]
    public async Task Sqlite_EnrollAsync_CatchesDbUpdateException_ReturnsAlreadyEnrolledAndRollsBack()
    {
        using var dbInit = CreateContext();
        await SeedBasicDataAsync(dbInit);

        // Đăng ký interceptor: pre-check DB trống nên vượt qua bước 1, 2, 3
        // Khi SaveChangesAsync bắt đầu, interceptor chèn bản ghi trùng để ép lỗi DbUpdateException
        var interceptor = new ForceDuplicateOnSaveInterceptor(_connection, 1, 1);
        using var db = CreateContext(interceptor);

        var service = new EnrollmentService(db);
        var result = await service.EnrollAsync(1, 1, EnrollmentActor.Student);

        Assert.False(result.Success);
        Assert.Equal(EnrollmentErrorCode.AlreadyEnrolled, result.ErrorCode);
        Assert.Equal("Bạn đã đăng ký lớp học phần này rồi.", result.Message);

        // Xác nhận trong DB không có bản ghi nào bị lưu dở dang do toàn bộ transaction đã được rollback
        using var dbVerify = CreateContext();
        var allEnrollments = await dbVerify.DangKyHocPhans
            .Where(dk => dk.SinhVienId == 1 && dk.LopHocPhanId == 1)
            .ToListAsync();
        Assert.Empty(allEnrollments);
    }

    // 2.c. Đăng ký lại sau hủy tái sử dụng đúng bản ghi DROPPED (cùng Id), không sinh bản ghi thứ hai
    [Fact]
    public async Task Sqlite_ReEnroll_ReusesDroppedRecord_SameId_WithoutUniqueViolation()
    {
        using var db = CreateContext();
        await SeedBasicDataAsync(db);

        var service = new EnrollmentService(db);

        // Lần 1: Sinh viên đăng ký
        var enroll1 = await service.EnrollAsync(1, 1, EnrollmentActor.Student);
        Assert.True(enroll1.Success);
        int originalRecordId = enroll1.Enrollment!.Id;

        // Lần 2: Sinh viên hủy đăng ký
        var dropResult = await service.DropAsync(1, 1, EnrollmentActor.Student);
        Assert.True(dropResult.Success);

        var droppedRecord = await db.DangKyHocPhans.AsNoTracking().FirstOrDefaultAsync(dk => dk.Id == originalRecordId);
        Assert.NotNull(droppedRecord);
        Assert.Equal(EnrollmentStatus.Dropped, droppedRecord.TrangThai);

        // Lần 3: Sinh viên đăng ký lại -> Phải tái sử dụng bản ghi DROPPED (cùng Id)
        var enrollAgain = await service.EnrollAsync(1, 1, EnrollmentActor.Student);
        Assert.True(enrollAgain.Success);
        Assert.Equal(originalRecordId, enrollAgain.Enrollment!.Id);
        Assert.Equal(EnrollmentStatus.Enrolled, enrollAgain.Enrollment.TrangThai);

        var totalRecords = await db.DangKyHocPhans.CountAsync(dk => dk.SinhVienId == 1 && dk.LopHocPhanId == 1);
        Assert.Equal(1, totalRecords);
    }

    // 2.d. DropAsync: Transaction rollback khi gặp lỗi giữa chừng, không lưu thay đổi dở dang
    [Fact]
    public async Task Sqlite_DropAsync_AtomicTransaction_RollsBackOnMiddleFailure()
    {
        using var db = CreateContext();
        await SeedBasicDataAsync(db);

        var service = new EnrollmentService(db);

        // Đăng ký cho sinh viên
        var enroll = await service.EnrollAsync(1, 1, EnrollmentActor.Student);
        Assert.True(enroll.Success);

        // Tạo điểm danh và bản ghi Diem rỗng
        db.DiemDanhs.Add(new DiemDanh
        {
            SinhVienId = 1,
            LopHocPhanId = 1,
            MonHocId = 1,
            NgayDiemDanh = DateTime.Today,
            TrangThai = AttendanceStatus.Present
        });
        db.Diems.Add(new Diem
        {
            SinhVienId = 1,
            LopHocPhanId = 1,
            MonHocId = 1,
            LanHoc = 1,
            LanThi = 1
        });
        await db.SaveChangesAsync();

        // Gắn interceptor gây lỗi giữa chừng khi SaveChanges trong DropAsync
        var failInterceptor = new FailOnSaveInterceptor();
        using var dbFailing = CreateContext(failInterceptor);
        var serviceFailing = new EnrollmentService(dbFailing);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await serviceFailing.DropAsync(1, 1, EnrollmentActor.Student);
        });

        // Kiểm tra sau khi rollback: bản ghi DangKy vẫn ENROLLED, Diem và DiemDanh vẫn nguyên vẹn
        using var dbVerify = CreateContext();
        var dk = await dbVerify.DangKyHocPhans.FirstAsync(d => d.SinhVienId == 1 && d.LopHocPhanId == 1);
        Assert.Equal(EnrollmentStatus.Enrolled, dk.TrangThai);
        Assert.Equal(1, await dbVerify.Diems.CountAsync(d => d.SinhVienId == 1 && d.LopHocPhanId == 1));
        Assert.Equal(1, await dbVerify.DiemDanhs.CountAsync(d => d.SinhVienId == 1 && d.LopHocPhanId == 1));
    }

    // 1.a. Đăng ký lớp A môn X, hủy, đăng ký lớp B môn X -> LanHoc=1 và HOC_LAN_DAU
    [Fact]
    public async Task Sqlite_LanHoc_DroppedClassA_DoesNotIncreaseLanHocForClassB()
    {
        using var db = CreateContext();
        await SeedBasicDataAsync(db);

        var service = new EnrollmentService(db);

        // 1. Đăng ký lớp 1 môn INT101
        var enrollA = await service.EnrollAsync(1, 1, EnrollmentActor.Student);
        Assert.True(enrollA.Success);
        Assert.Equal(1, enrollA.Enrollment!.LanHoc);
        Assert.Equal("HOC_LAN_DAU", enrollA.Enrollment.LoaiDangKy);

        // 2. Hủy đăng ký lớp 1 môn INT101
        var dropA = await service.DropAsync(1, 1, EnrollmentActor.Student);
        Assert.True(dropA.Success);

        // 3. Đăng ký lớp 2 cùng môn INT101 -> Vì lớp A đã DROPPED (chưa học thật), LanHoc vẫn phải là 1, HOC_LAN_DAU
        var enrollB = await service.EnrollAsync(1, 2, EnrollmentActor.Student);
        Assert.True(enrollB.Success);
        Assert.Equal(1, enrollB.Enrollment!.LanHoc);
        Assert.Equal("HOC_LAN_DAU", enrollB.Enrollment.LoaiDangKy);
    }

    // 1.b. Đã có Diem không đạt ở môn X, đăng ký lớp mới cùng môn -> HOC_LAI và LanHoc=2
    [Fact]
    public async Task Sqlite_LanHoc_PreviousFailingDiem_SetsHocLaiAndLanHoc2()
    {
        using var db = CreateContext();
        await SeedBasicDataAsync(db);

        // Chèn lịch sử điểm không đạt của môn 1
        db.Diems.Add(new Diem
        {
            SinhVienId = 1,
            MonHocId = 1,
            LanHoc = 1,
            LanThi = 1,
            DiemChuyenCan = 3.0,
            DiemGiuaKy = 3.0,
            DiemCuoiKy = 3.0,
            TrangThai = "KHONG_DAT",
            SuDungTinhTichLuy = false
        });
        await db.SaveChangesAsync();

        var service = new EnrollmentService(db);
        var enroll = await service.EnrollAsync(1, 1, EnrollmentActor.Student);

        Assert.True(enroll.Success);
        Assert.Equal(2, enroll.Enrollment!.LanHoc);
        Assert.Equal("HOC_LAI", enroll.Enrollment.LoaiDangKy);
        Assert.Contains("Đăng ký học lại do lần 1 không đạt", enroll.Enrollment.GhiChu);
    }

    // 1.c. Hủy rồi đăng ký lại đúng lớp cũ giữ LanHoc đúng
    [Fact]
    public async Task Sqlite_LanHoc_DropAndReEnrollSameClass_PreservesCorrectLanHoc()
    {
        using var db = CreateContext();
        await SeedBasicDataAsync(db);

        // Giả sử sinh viên học lại lần 2 do đã có Diem không đạt
        db.Diems.Add(new Diem
        {
            SinhVienId = 1,
            MonHocId = 1,
            LanHoc = 1,
            LanThi = 1,
            DiemChuyenCan = 3.0,
            DiemGiuaKy = 3.0,
            DiemCuoiKy = 3.0,
            TrangThai = "KHONG_DAT"
        });
        await db.SaveChangesAsync();

        var service = new EnrollmentService(db);

        // Đăng ký lần 2
        var enroll1 = await service.EnrollAsync(1, 1, EnrollmentActor.Student);
        Assert.True(enroll1.Success);
        Assert.Equal(2, enroll1.Enrollment!.LanHoc);
        Assert.Equal("HOC_LAI", enroll1.Enrollment.LoaiDangKy);

        // Hủy đăng ký
        var drop = await service.DropAsync(1, 1, EnrollmentActor.Student);
        Assert.True(drop.Success);

        // Đăng ký lại lớp cũ -> Vẫn phải giữ đúng LanHoc = 2 và HOC_LAI
        var reEnroll = await service.EnrollAsync(1, 1, EnrollmentActor.Student);
        Assert.True(reEnroll.Success);
        Assert.Equal(2, reEnroll.Enrollment!.LanHoc);
        Assert.Equal("HOC_LAI", reEnroll.Enrollment.LoaiDangKy);
    }
}
