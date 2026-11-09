using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BTL_StudentMGMT.Tests;

public class SecurityAuthorizationTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetChiTietBuoiHoc_WhenCalledForStudent_MustNotReturnRoster()
    {
        using var db = CreateInMemoryDbContext();

        var monHoc = new MonHoc { Id = 1, MaMon = "INT301", TenMon = "Lập trình Web", SoTinChi = 3 };
        db.MonHocs.Add(monHoc);

        var lhp = new LopHocPhan
        {
            Id = 5,
            MaLopHocPhan = "INT301.01",
            TenLopHocPhan = "Lập trình Web",
            MonHocId = 1,
            Thu = "Thứ 2",
            TietBatDau = 1,
            SoTiet = 3,
            TrangThai = "OPEN"
        };
        db.LopHocPhans.Add(lhp);

        var sv1 = new SinhVien { Id = 101, MaSinhVien = "SV101", HoTen = "Sinh Vien A" };
        var sv2 = new SinhVien { Id = 102, MaSinhVien = "SV102", HoTen = "Sinh Vien B" };
        db.SinhViens.AddRange(sv1, sv2);

        db.DangKyHocPhans.AddRange(
            new DangKyHocPhan { SinhVienId = 101, LopHocPhanId = lhp.Id, TrangThai = "ENROLLED" },
            new DangKyHocPhan { SinhVienId = 102, LopHocPhanId = lhp.Id, TrangThai = "ENROLLED" }
        );

        db.DiemDanhs.Add(new DiemDanh
        {
            SinhVienId = 101,
            LopHocPhanId = lhp.Id,
            NgayDiemDanh = new DateTime(2026, 11, 2),
            TrangThai = AttendanceStatus.Present
        });

        await db.SaveChangesAsync();

        var timetableService = new TimetableService(db);

        // Khi gọi cho Sinh viên (currentSinhVienId = 101):
        var studentResult = await timetableService.GetChiTietBuoiHocAsync(lhp.Id, new DateTime(2026, 11, 2), currentSinhVienId: 101);
        Assert.NotNull(studentResult);
        Assert.Empty(studentResult.DanhSachSinhVien); // TUYỆT ĐỐI không có roster
        Assert.Equal("Có mặt", studentResult.TrangThaiDiemDanhCuaSinhVien); // Trả đúng trạng thái của chính SV

        // Khi gọi cho Giảng viên (currentSinhVienId = null, currentGiangVienId = 1):
        var lecturerResult = await timetableService.GetChiTietBuoiHocAsync(lhp.Id, new DateTime(2026, 11, 2), currentGiangVienId: 1);
        Assert.NotNull(lecturerResult);
        Assert.Equal(2, lecturerResult.DanhSachSinhVien.Count); // Giảng viên được thấy roster
    }
}
