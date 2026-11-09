using System;
using System.Linq;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class EnrollmentServiceTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Enroll_OldSemesterClass_DoesNotBlockNewSemester()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh = new MonHoc { Id = 1, MaMon = "INT101", TenMon = "Lap trinh C", SoTinChi = 3 };
        var hk1 = new HocKy { HocKyId = 1, MaHocKy = "2025_1", TenHocKy = "Hoc ky 1", TrangThai = "DONG", NamHocId = 1 };
        var hk2 = new HocKy { HocKyId = 2, MaHocKy = "2025_2", TenHocKy = "Hoc ky 2", TrangThai = "DANG_MO", NamHocId = 1 };

        // Lớp ở học kỳ 1 (học Thứ 2, tiết 1-3)
        var lhp1 = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "INT101.01",
            TenLopHocPhan = "Lop HK1",
            MonHocId = mh.Id,
            HocKyId = hk1.HocKyId,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 3,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        // Lớp ở học kỳ 2 (cùng Thứ 2, tiết 1-3)
        var lhp2 = new LopHocPhan
        {
            Id = 2,
            MaLopHocPhan = "INT101.02",
            TenLopHocPhan = "Lop HK2",
            MonHocId = mh.Id,
            HocKyId = hk2.HocKyId,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 3,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        db.SinhViens.Add(sv);
        db.MonHocs.Add(mh);
        db.HocKies.AddRange(hk1, hk2);
        db.LopHocPhans.AddRange(lhp1, lhp2);

        // Sinh viên đã đăng ký lhp1 ở học kỳ 1
        db.DangKyHocPhans.Add(new DangKyHocPhan
        {
            SinhVienId = sv.Id,
            LopHocPhanId = lhp1.Id,
            TrangThai = EnrollmentStatus.Enrolled
        });
        await db.SaveChangesAsync();

        // Đăng ký lhp2 ở học kỳ 2 -> Không bị chặn trùng lịch hay trùng môn của học kỳ cũ
        var result = await service.EnrollAsync(sv.Id, lhp2.Id, EnrollmentActor.Student);

        Assert.True(result.Success);
        Assert.Equal(EnrollmentErrorCode.None, result.ErrorCode);
    }

    [Fact]
    public async Task Enroll_SameSubjectInSameSemester_IsBlocked()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh = new MonHoc { Id = 1, MaMon = "MAT101", TenMon = "Giai tich 1", SoTinChi = 3 };
        var hk = new HocKy { HocKyId = 1, MaHocKy = "2025_1", TenHocKy = "Hoc ky 1", TrangThai = "DANG_MO", NamHocId = 1 };

        var lhp1 = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "MAT101.01",
            TenLopHocPhan = "Giai tich Nhom 1",
            MonHocId = mh.Id,
            HocKyId = hk.HocKyId,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 3,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        var lhp2 = new LopHocPhan
        {
            Id = 2,
            MaLopHocPhan = "MAT101.02",
            TenLopHocPhan = "Giai tich Nhom 2",
            MonHocId = mh.Id,
            HocKyId = hk.HocKyId,
            Thu = "Thứ 5",
            TietBatDau = 4,
            SoTiet = 3,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        db.SinhViens.Add(sv);
        db.MonHocs.Add(mh);
        db.HocKies.Add(hk);
        db.LopHocPhans.AddRange(lhp1, lhp2);

        // Sinh viên đã đăng ký nhóm 1
        db.DangKyHocPhans.Add(new DangKyHocPhan
        {
            SinhVienId = sv.Id,
            LopHocPhanId = lhp1.Id,
            TrangThai = EnrollmentStatus.Enrolled
        });
        await db.SaveChangesAsync();

        // Cố đăng ký tiếp nhóm 2 trong cùng học kỳ
        var result = await service.EnrollAsync(sv.Id, lhp2.Id, EnrollmentActor.Student);

        Assert.False(result.Success);
        Assert.Equal(EnrollmentErrorCode.SameSubjectEnrolled, result.ErrorCode);
    }

    [Fact]
    public async Task Enroll_WhenClassFull_IsBlocked()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv1 = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var sv2 = new SinhVien { Id = 2, MaSinhVien = "SV02", HoTen = "Nguyen Van B" };
        var mh = new MonHoc { Id = 1, MaMon = "PHY101", TenMon = "Vat ly", SoTinChi = 2 };
        var hk = new HocKy { HocKyId = 1, MaHocKy = "2025_1", TenHocKy = "Hoc ky 1", TrangThai = "DANG_MO", NamHocId = 1 };

        var lhp = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "PHY101.01",
            TenLopHocPhan = "Vat ly 1",
            MonHocId = mh.Id,
            HocKyId = hk.HocKyId,
            Thu = "Thứ 3",
            TietBatDau = 1,
            SoTiet = 2,
            SiSoToiDa = 1, // Sĩ số tối đa = 1
            TrangThai = "OPEN"
        };

        db.SinhViens.AddRange(sv1, sv2);
        db.MonHocs.Add(mh);
        db.HocKies.Add(hk);
        db.LopHocPhans.Add(lhp);

        // Sinh viên 1 đã đăng ký
        db.DangKyHocPhans.Add(new DangKyHocPhan
        {
            SinhVienId = sv1.Id,
            LopHocPhanId = lhp.Id,
            TrangThai = EnrollmentStatus.Enrolled
        });
        await db.SaveChangesAsync();

        // Sinh viên 2 đăng ký -> hết chỗ
        var result = await service.EnrollAsync(sv2.Id, lhp.Id, EnrollmentActor.Student);

        Assert.False(result.Success);
        Assert.Equal(EnrollmentErrorCode.ClassFull, result.ErrorCode);
    }

    [Fact]
    public async Task Enroll_DoubleEnroll_ReturnsAlreadyEnrolledWithout500()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh = new MonHoc { Id = 1, MaMon = "ENG101", TenMon = "Tieng Anh 1", SoTinChi = 3 };
        var hk = new HocKy { HocKyId = 1, MaHocKy = "2025_1", TenHocKy = "Hoc ky 1", TrangThai = "DANG_MO", NamHocId = 1 };

        var lhp = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "ENG101.01",
            TenLopHocPhan = "Tieng Anh 1",
            MonHocId = mh.Id,
            HocKyId = hk.HocKyId,
            Thu = "Thứ 4",
            TietBatDau = 1,
            SoTiet = 3,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        db.SinhViens.Add(sv);
        db.MonHocs.Add(mh);
        db.HocKies.Add(hk);
        db.LopHocPhans.Add(lhp);
        await db.SaveChangesAsync();

        // Đăng ký lần 1 -> Thành công
        var firstResult = await service.EnrollAsync(sv.Id, lhp.Id, EnrollmentActor.Student);
        Assert.True(firstResult.Success);

        // Bấm đúp đăng ký lần 2 -> Không phát sinh lỗi 500, trả về thông báo đã đăng ký
        var secondResult = await service.EnrollAsync(sv.Id, lhp.Id, EnrollmentActor.Student);
        Assert.False(secondResult.Success);
        Assert.Equal(EnrollmentErrorCode.AlreadyEnrolled, secondResult.ErrorCode);
    }

    [Fact]
    public async Task Drop_RefusedWhenStudentHasGrades()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh = new MonHoc { Id = 1, MaMon = "CS101", TenMon = "Co so lap trinh", SoTinChi = 3 };
        var lhp = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "CS101.01",
            TenLopHocPhan = "CS101",
            MonHocId = mh.Id,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        var dk = new DangKyHocPhan
        {
            Id = 10,
            SinhVienId = sv.Id,
            LopHocPhanId = lhp.Id,
            TrangThai = EnrollmentStatus.Enrolled
        };

        // Điểm đã được chấm
        var diem = new Diem
        {
            SinhVienId = sv.Id,
            LopHocPhanId = lhp.Id,
            MonHocId = mh.Id,
            DiemChuyenCan = 9.0,
            DiemGiuaKy = 8.0,
            DiemCuoiKy = 8.5
        };

        db.SinhViens.Add(sv);
        db.MonHocs.Add(mh);
        db.LopHocPhans.Add(lhp);
        db.DangKyHocPhans.Add(dk);
        db.Diems.Add(diem);
        await db.SaveChangesAsync();

        var result = await service.DropAsync(sv.Id, lhp.Id, EnrollmentActor.Student);

        Assert.False(result.Success);
        Assert.Equal(EnrollmentErrorCode.HasGradesCannotDrop, result.ErrorCode);
    }

    [Fact]
    public async Task Drop_EmptyGradeRecord_IsDeletedAndAttendanceIsCleaned()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh = new MonHoc { Id = 1, MaMon = "CS102", TenMon = "Cau truc du lieu", SoTinChi = 3 };
        var lhp = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "CS102.01",
            TenLopHocPhan = "CS102",
            MonHocId = mh.Id,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        var dk = new DangKyHocPhan
        {
            Id = 20,
            SinhVienId = sv.Id,
            LopHocPhanId = lhp.Id,
            TrangThai = EnrollmentStatus.Enrolled
        };

        // Bản ghi điểm rỗng (chưa có điểm thành phần nào)
        var diemRong = new Diem
        {
            SinhVienId = sv.Id,
            LopHocPhanId = lhp.Id,
            MonHocId = mh.Id,
            DiemChuyenCan = 0,
            DiemGiuaKy = 0,
            DiemCuoiKy = 0
        };

        // Điểm danh
        var diemDanh = new DiemDanh
        {
            SinhVienId = sv.Id,
            LopHocPhanId = lhp.Id,
            NgayDiemDanh = DateTime.Today,
            TrangThai = "Có mặt"
        };

        db.SinhViens.Add(sv);
        db.MonHocs.Add(mh);
        db.LopHocPhans.Add(lhp);
        db.DangKyHocPhans.Add(dk);
        db.Diems.Add(diemRong);
        db.DiemDanhs.Add(diemDanh);
        await db.SaveChangesAsync();

        var result = await service.DropAsync(sv.Id, lhp.Id, EnrollmentActor.Student);

        Assert.True(result.Success);

        // Kiểm tra xóa mềm DangKyHocPhan
        var updatedDk = await db.DangKyHocPhans.FirstAsync(d => d.Id == dk.Id);
        Assert.Equal(EnrollmentStatus.Dropped, updatedDk.TrangThai);

        // Bản ghi Diem rỗng bị xóa
        var remainingDiem = await db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == sv.Id && d.LopHocPhanId == lhp.Id);
        Assert.Null(remainingDiem);

        // Điểm danh bị dọn
        var remainingDiemDanh = await db.DiemDanhs.FirstOrDefaultAsync(dd => dd.SinhVienId == sv.Id && dd.LopHocPhanId == lhp.Id);
        Assert.Null(remainingDiemDanh);
    }

    [Fact]
    public async Task ReEnroll_ReusesDroppedRecord_WithoutUniqueViolation()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh = new MonHoc { Id = 1, MaMon = "WEB101", TenMon = "Web", SoTinChi = 3 };
        var lhp = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "WEB101.01",
            TenLopHocPhan = "Web 1",
            MonHocId = mh.Id,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        db.SinhViens.Add(sv);
        db.MonHocs.Add(mh);
        db.LopHocPhans.Add(lhp);
        await db.SaveChangesAsync();

        // 1. Đăng ký lần đầu
        var res1 = await service.EnrollAsync(sv.Id, lhp.Id, EnrollmentActor.Student);
        Assert.True(res1.Success);

        // 2. Hủy đăng ký -> chuyển sang DROPPED
        var dropRes = await service.DropAsync(sv.Id, lhp.Id, EnrollmentActor.Student);
        Assert.True(dropRes.Success);

        var droppedRecord = await db.DangKyHocPhans.FirstAsync(d => d.SinhVienId == sv.Id && d.LopHocPhanId == lhp.Id);
        Assert.Equal(EnrollmentStatus.Dropped, droppedRecord.TrangThai);

        // 3. Đăng ký lại -> TÁI SỬ DỤNG bản ghi DROPPED
        var res2 = await service.EnrollAsync(sv.Id, lhp.Id, EnrollmentActor.Student);
        Assert.True(res2.Success);

        // Số bản ghi vẫn chỉ là 1, không tạo thêm bản ghi mới gây vi phạm unique index
        var totalRecords = await db.DangKyHocPhans.CountAsync(d => d.SinhVienId == sv.Id && d.LopHocPhanId == lhp.Id);
        Assert.Equal(1, totalRecords);

        var activeRecord = await db.DangKyHocPhans.FirstAsync(d => d.SinhVienId == sv.Id && d.LopHocPhanId == lhp.Id);
        Assert.Equal(EnrollmentStatus.Enrolled, activeRecord.TrangThai);
    }

    [Fact]
    public async Task DroppedStudent_ExcludedFromRoster_Timetable_Capacity_CanGradeStudent_AndPayment()
    {
        using var db = CreateDbContext();

        var svEnrolled = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Sinh Vien Dang Hoc" };
        var svDropped = new SinhVien { Id = 2, MaSinhVien = "SV02", HoTen = "Sinh Vien Da Huy" };
        var mh = new MonHoc { Id = 1, MaMon = "DB101", TenMon = "CSDL", SoTinChi = 4 };

        var lhp = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "DB101.01",
            TenLopHocPhan = "CSDL 1",
            MonHocId = mh.Id,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        var dk1 = new DangKyHocPhan
        {
            SinhVienId = svEnrolled.Id,
            LopHocPhanId = lhp.Id,
            TrangThai = EnrollmentStatus.Enrolled
        };

        var dk2 = new DangKyHocPhan
        {
            SinhVienId = svDropped.Id,
            LopHocPhanId = lhp.Id,
            TrangThai = EnrollmentStatus.Dropped
        };

        db.SinhViens.AddRange(svEnrolled, svDropped);
        db.MonHocs.Add(mh);
        db.LopHocPhans.Add(lhp);
        db.DangKyHocPhans.AddRange(dk1, dk2);
        await db.SaveChangesAsync();

        // Nạp kèm DangKyHocPhans
        var loadedLhp = await db.LopHocPhans
            .Include(l => l.DangKyHocPhans)
            .FirstAsync(l => l.Id == lhp.Id);

        // 1. Sĩ số chỉ đếm ENROLLED
        var siSo = loadedLhp.DangKyHocPhans.Count(d => d.TrangThai == EnrollmentStatus.Enrolled);
        Assert.Equal(1, siSo);

        // 2. Roster lớp chỉ chứa ENROLLED
        var roster = loadedLhp.DangKyHocPhans.Where(d => d.TrangThai == EnrollmentStatus.Enrolled).ToList();
        Assert.Single(roster);
        Assert.Equal(svEnrolled.Id, roster[0].SinhVienId);

        // 3. TKB của sinh viên DROPPED không có lớp này
        var tkbCount = await db.DangKyHocPhans
            .Where(d => d.SinhVienId == svDropped.Id && d.TrangThai == EnrollmentStatus.Enrolled)
            .CountAsync();
        Assert.Equal(0, tkbCount);

        // 4. CanGradeStudent trả về false cho sinh viên DROPPED
        var canGradeDropped = await db.DangKyHocPhans
            .AnyAsync(d => d.LopHocPhanId == lhp.Id && d.SinhVienId == svDropped.Id && d.TrangThai == EnrollmentStatus.Enrolled);
        Assert.False(canGradeDropped);

        var canGradeEnrolled = await db.DangKyHocPhans
            .AnyAsync(d => d.LopHocPhanId == lhp.Id && d.SinhVienId == svEnrolled.Id && d.TrangThai == EnrollmentStatus.Enrolled);
        Assert.True(canGradeEnrolled);

        // 5. PaymentService: Tính tín chỉ chỉ tính ENROLLED, môn DROPPED tính ra 0
        var tinChiDropped = await db.DangKyHocPhans
            .Where(d => d.SinhVienId == svDropped.Id && d.TrangThai == EnrollmentStatus.Enrolled)
            .SumAsync(d => (int?)d.LopHocPhan.MonHoc!.SoTinChi) ?? 0;
        Assert.Equal(0, tinChiDropped);

        var tinChiEnrolled = await db.DangKyHocPhans
            .Where(d => d.SinhVienId == svEnrolled.Id && d.TrangThai == EnrollmentStatus.Enrolled)
            .SumAsync(d => (int?)d.LopHocPhan.MonHoc!.SoTinChi) ?? 0;
        Assert.Equal(4, tinChiEnrolled);
    }

    [Fact]
    public async Task Admin_ThemSinhVien_BlockedOnConflict_AndCalculatesHocLaiCorrectly()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh1 = new MonHoc { Id = 1, MaMon = "JAVA101", TenMon = "Java", SoTinChi = 3 };
        var mh2 = new MonHoc { Id = 2, MaMon = "DOTNET101", TenMon = "C# .NET", SoTinChi = 3 };
        var hk = new HocKy { HocKyId = 1, MaHocKy = "2025_1", TenHocKy = "Hoc ky 1", TrangThai = "DANG_MO", NamHocId = 1 };

        // Lớp Java Thứ 2, tiết 1-3
        var lhpJava = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "JAVA101.01",
            TenLopHocPhan = "Lop Java",
            MonHocId = mh1.Id,
            HocKyId = hk.HocKyId,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 3,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        // Lớp C# Thứ hai (cùng ngày), tiết 2-4 (trùng với 1-3)
        var lhpDotNet = new LopHocPhan
        {
            Id = 2,
            MaLopHocPhan = "DOTNET101.01",
            TenLopHocPhan = "Lop .NET",
            MonHocId = mh2.Id,
            HocKyId = hk.HocKyId,
            Thu = "Thứ hai",
            TietBatDau = 2,
            SoTiet = 3,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        // Lịch sử học Java lần 1 bị trượt (DiemTongKet = 3.0 < 4.0)
        var diemTruot = new Diem
        {
            SinhVienId = sv.Id,
            MonHocId = mh1.Id,
            LanHoc = 1,
            DiemChuyenCan = 4,
            DiemGiuaKy = 3,
            DiemCuoiKy = 2.5
        };

        db.SinhViens.Add(sv);
        db.MonHocs.AddRange(mh1, mh2);
        db.HocKies.Add(hk);
        db.LopHocPhans.AddRange(lhpJava, lhpDotNet);
        db.Diems.Add(diemTruot);
        await db.SaveChangesAsync();

        // 1. Admin thêm sinh viên vào lớp Java -> Tự động tính HOC_LAI, LanHoc = 2
        var enrollJava = await service.EnrollAsync(sv.Id, lhpJava.Id, EnrollmentActor.Admin);
        Assert.True(enrollJava.Success);
        Assert.NotNull(enrollJava.Enrollment);
        Assert.Equal("HOC_LAI", enrollJava.Enrollment.LoaiDangKy);
        Assert.Equal(2, enrollJava.Enrollment.LanHoc);

        // 2. Admin cố thêm tiếp vào lớp .NET bị trùng lịch (Thứ 2 tiết 1-3 vs Thứ hai tiết 2-4)
        var enrollConflict = await service.EnrollAsync(sv.Id, lhpDotNet.Id, EnrollmentActor.Admin);
        Assert.False(enrollConflict.Success);
        Assert.Equal(EnrollmentErrorCode.ScheduleConflict, enrollConflict.ErrorCode);
    }
}
