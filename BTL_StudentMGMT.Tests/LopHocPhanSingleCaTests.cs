using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BTL_StudentMGMT.Controllers;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class LopHocPhanSingleCaTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private QuanLyLopHocPhanController CreateController(ApplicationDbContext db)
    {
        var mockAttendance = new Mock<IAttendanceService>();
        var mockEnrollment = new Mock<IEnrollmentService>();

        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        var controller = new QuanLyLopHocPhanController(db, mockAttendance.Object, mockEnrollment.Object)
        {
            TempData = tempData
        };
        return controller;
    }

    [Theory]
    [InlineData(1, 1, 2)]
    [InlineData(2, 3, 2)]
    [InlineData(3, 5, 2)]
    [InlineData(4, 7, 2)]
    [InlineData(5, 9, 2)]
    public void CaHocHelper_GetTietTuCa_MapsCorrectly(int ca, int expectedStart, int expectedCount)
    {
        var (start, count) = CaHocHelper.GetTietTuCa(ca);
        Assert.Equal(expectedStart, start);
        Assert.Equal(expectedCount, count);
    }

    [Theory]
    [InlineData(1, 2, true, 1)]
    [InlineData(3, 2, true, 2)]
    [InlineData(5, 2, true, 3)]
    [InlineData(7, 2, true, 4)]
    [InlineData(9, 2, true, 5)]
    [InlineData(1, 3, false, null)] // Legacy data (3 periods)
    [InlineData(1, 4, false, null)] // Legacy data (4 periods)
    [InlineData(7, 3, false, null)] // Legacy data (3 periods)
    [InlineData(2, 2, false, null)] // Not beginning of shift
    [InlineData(4, 2, false, null)] // Not beginning of shift
    public void CaHocHelper_IsSingleCa_And_GetCaChuan_IdentifiesCorrectly(int start, int count, bool expectedSingle, int? expectedCa)
    {
        Assert.Equal(expectedSingle, CaHocHelper.IsSingleCa(start, count));
        Assert.Equal(expectedCa, CaHocHelper.GetCaChuan(start, count));
    }

    [Fact]
    public void LopHocPhanFormViewModel_Validation_RequiresValidCa()
    {
        // 1. Ca null -> Invalid
        var vmNull = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Web",
            MonHocId = 1,
            Ca = null
        };
        var resultsNull = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(vmNull, new ValidationContext(vmNull), resultsNull, true));
        Assert.Contains(resultsNull, r => r.MemberNames.Contains(nameof(vmNull.Ca)));

        // 2. Ca = 0 -> Invalid
        var vm0 = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Web",
            MonHocId = 1,
            Ca = 0
        };
        var results0 = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(vm0, new ValidationContext(vm0), results0, true));
        Assert.Contains(results0, r => r.MemberNames.Contains(nameof(vm0.Ca)));

        // 3. Ca = 6 -> Invalid
        var vm6 = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Web",
            MonHocId = 1,
            Ca = 6
        };
        var results6 = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(vm6, new ValidationContext(vm6), results6, true));
        Assert.Contains(results6, r => r.MemberNames.Contains(nameof(vm6.Ca)));

        // 4. Ca = 3 -> Valid
        var vm3 = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Web",
            MonHocId = 1,
            Ca = 3
        };
        var results3 = new List<ValidationResult>();
        Assert.True(Validator.TryValidateObject(vm3, new ValidationContext(vm3), results3, true));
    }

    [Fact]
    public async Task Controller_Create_WithCa3_PersistsTietBatDau5_SoTiet2()
    {
        using var db = CreateDbContext();
        var controller = CreateController(db);

        var model = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Lập trình Web",
            MonHocId = 1,
            HocKyId = 1,
            Thu = "Thứ 4",
            Ca = 3,
            PhongHoc = "P.302-A1"
        };

        var result = await controller.Create(model);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var saved = await db.LopHocPhans.FirstOrDefaultAsync(l => l.MaLopHocPhan == "INT301.01");
        Assert.NotNull(saved);
        Assert.Equal(5, saved.TietBatDau);
        Assert.Equal(2, saved.SoTiet);
        Assert.Equal(3, saved.Ca);
    }

    [Fact]
    public async Task Controller_Create_WithInvalidCa_RejectsAndReturnsForm()
    {
        using var db = CreateDbContext();
        var controller = CreateController(db);

        var model = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Lập trình Web",
            MonHocId = 1,
            Ca = 6 // Ca không hợp lệ
        };

        var result = await controller.Create(model);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Form", viewResult.ViewName);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("Ca"));
    }

    [Fact]
    public async Task Controller_CreateAndEdit_InstructorConflict_DetectedAndReportsDetails()
    {
        using var db = CreateDbContext();
        var controller = CreateController(db);

        // Đã có lớp 1: Giảng viên 10, Thứ 3, Ca 1 (Tiết 1-2)
        db.LopHocPhans.Add(new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "INT101.01",
            TenLopHocPhan = "Cơ sở lập trình",
            MonHocId = 1,
            GiangVienId = 10,
            Thu = "Thứ 3",
            TietBatDau = 1,
            SoTiet = 2,
            TrangThai = "OPEN"
        });
        await db.SaveChangesAsync();

        // Thử tạo lớp 2 cùng Giảng viên 10, Thứ 3, Ca 1
        var newClass = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT102.01",
            TenLopHocPhan = "Lập trình nâng cao",
            MonHocId = 2,
            GiangVienId = 10,
            Thu = "Thứ 3",
            Ca = 1
        };

        var resultCreate = await controller.Create(newClass);
        Assert.IsType<ViewResult>(resultCreate);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("GiangVienId"));
        var errorMsg = controller.ModelState["GiangVienId"]!.Errors[0].ErrorMessage;
        Assert.Contains("INT101.01", errorMsg);
        Assert.Contains("Thứ 3", errorMsg);
        Assert.Contains("Ca 1", errorMsg);

        // Edit chính lớp 1 -> KHÔNG bị báo trùng với chính mình!
        var editSelf = new LopHocPhanFormViewModel
        {
            Id = 1,
            MaLopHocPhan = "INT101.01",
            TenLopHocPhan = "Cơ sở lập trình (Đổi tên)",
            MonHocId = 1,
            GiangVienId = 10,
            Thu = "Thứ 3",
            Ca = 1
        };

        var controllerEdit = CreateController(db);
        var resultEdit = await controllerEdit.Edit(1, editSelf);
        var redirectEdit = Assert.IsType<RedirectToActionResult>(resultEdit);
        Assert.Equal("Index", redirectEdit.ActionName);
    }

    [Fact]
    public async Task Controller_RoomConflict_CheckedSameSemester_AllowedDifferentSemester()
    {
        using var db = CreateDbContext();
        var controller = CreateController(db);

        // Đã có lớp 1: Phòng P.302, Thứ 4, Ca 2 (Tiết 3-4), Học kỳ 1
        db.LopHocPhans.Add(new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "INT201.01",
            TenLopHocPhan = "CTDL & GT",
            MonHocId = 1,
            HocKyId = 1,
            Thu = "Thứ 4",
            TietBatDau = 3,
            SoTiet = 2,
            PhongHoc = "P.302",
            TrangThai = "OPEN"
        });
        await db.SaveChangesAsync();

        // 1. Thử tạo lớp 2: Cùng phòng P.302, Thứ 4, Ca 2, CÙNG học kỳ 1 -> Bị trùng phòng
        var modelConflict = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT202.01",
            TenLopHocPhan = "OOP",
            MonHocId = 2,
            HocKyId = 1,
            Thu = "Thứ 4",
            Ca = 2,
            PhongHoc = "P.302"
        };

        var resultConflict = await controller.Create(modelConflict);
        Assert.IsType<ViewResult>(resultConflict);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("PhongHoc"));
        var err = controller.ModelState["PhongHoc"]!.Errors[0].ErrorMessage;
        Assert.Contains("P.302", err);
        Assert.Contains("INT201.01", err);
        Assert.Contains("Ca 2", err);

        // 2. Thử tạo lớp 3: Cùng phòng P.302, Thứ 4, Ca 2, nhưng KHÁC học kỳ (Học kỳ 2) -> Cho phép
        var controllerDiffHk = CreateController(db);
        var modelDiffHk = new LopHocPhanFormViewModel
        {
            MaLopHocPhan = "INT203.01",
            TenLopHocPhan = "Web Service",
            MonHocId = 3,
            HocKyId = 2, // Khác học kỳ
            Thu = "Thứ 4",
            Ca = 2,
            PhongHoc = "P.302"
        };

        var resultDiffHk = await controllerDiffHk.Create(modelDiffHk);
        var redirectDiffHk = Assert.IsType<RedirectToActionResult>(resultDiffHk);
        Assert.Equal("Index", redirectDiffHk.ActionName);
    }

    [Fact]
    public async Task Controller_EditGet_LegacyData_LeavesCaNull_ForUserSelection()
    {
        using var db = CreateDbContext();
        var controller = CreateController(db);

        // Dữ liệu cũ vi phạm (Tiết 1, Số tiết 3 - không khớp ca chuẩn)
        var legacy = new LopHocPhan
        {
            Id = 99,
            MaLopHocPhan = "LEGACY.01",
            TenLopHocPhan = "Lớp học phần cũ",
            MonHocId = 1,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 3, // Vi phạm
            TrangThai = "OPEN"
        };
        db.LopHocPhans.Add(legacy);
        await db.SaveChangesAsync();

        var result = await controller.Edit(99);
        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<LopHocPhanFormViewModel>(viewResult.Model);

        Assert.Null(vm.Ca); // Dropdown sẽ để trống để người dùng chọn lại ca
        Assert.False(CaHocHelper.IsSingleCa(vm.TietBatDau, vm.SoTiet));
    }

    [Fact]
    public async Task EnrollmentService_ConflictMessage_UsesStandardizedCaString()
    {
        using var db = CreateDbContext();
        var service = new EnrollmentService(db);

        var sv = new SinhVien { Id = 1, MaSinhVien = "SV01", HoTen = "Nguyen Van A" };
        var mh1 = new MonHoc { Id = 1, MaMon = "INT101", TenMon = "Web", SoTinChi = 3 };
        var mh2 = new MonHoc { Id = 2, MaMon = "INT102", TenMon = "CSDL", SoTinChi = 3 };
        var hk = new HocKy { HocKyId = 1, MaHocKy = "2026_1", TenHocKy = "Học kỳ 1", TrangThai = "DANG_MO" };

        var lhp1 = new LopHocPhan
        {
            Id = 1,
            MaLopHocPhan = "INT101.01",
            TenLopHocPhan = "Lập trình Web",
            MonHocId = 1,
            HocKyId = 1,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 2,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        var lhp2 = new LopHocPhan
        {
            Id = 2,
            MaLopHocPhan = "INT102.01",
            TenLopHocPhan = "Cơ sở dữ liệu",
            MonHocId = 2,
            HocKyId = 1,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 2,
            SiSoToiDa = 30,
            TrangThai = "OPEN"
        };

        db.SinhViens.Add(sv);
        db.MonHocs.AddRange(mh1, mh2);
        db.HocKies.Add(hk);
        db.LopHocPhans.AddRange(lhp1, lhp2);
        await db.SaveChangesAsync();

        // 1. Đăng ký lớp 1
        var res1 = await service.EnrollAsync(1, 1, EnrollmentActor.Student);
        Assert.True(res1.Success);

        // 2. Đăng ký lớp 2 trùng Thứ 2, Ca 1 -> Bị từ chối trùng lịch
        var res2 = await service.EnrollAsync(1, 2, EnrollmentActor.Student);
        Assert.False(res2.Success);
        Assert.Equal(EnrollmentErrorCode.ScheduleConflict, res2.ErrorCode);
        Assert.Contains("Thứ 2, Ca 1 (07:00 – 09:30)", res2.Message);
    }
}
