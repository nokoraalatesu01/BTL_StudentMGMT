using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BTL_StudentMGMT.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // 1. Tự động áp dụng Migration an toàn, có logging chi tiết
        try
        {
            logger.LogInformation("Bắt đầu kiểm tra và áp dụng EF Core Migrations...");
            await db.Database.MigrateAsync();
            logger.LogInformation("Áp dụng EF Core Migrations thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi nghiêm trọng khi áp dụng EF Migration. Dừng quá trình Seed để tránh sai lệch dữ liệu.");
            throw; // Không tiếp tục seed khi database chưa đạt schema chuẩn
        }

        // 2. Seed Roles (Idempotent)
        var roles = new[] { "Admin", "GiangVien", "SinhVien", "PGV" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 3. Seed Users
        var adminUser = await CreateUser(userManager, "admin", "Quản trị viên", "Admin@123", "Admin");
        var giangVienUser = await CreateUser(userManager, "gv01", "PGS.TS. Trần Đình Khang", "GiangVien@123", "GiangVien");
        var sinhVienUser = await CreateUser(userManager, "sv01", "Nguyễn Văn Hưng", "SinhVien@123", "SinhVien");
        await CreateUser(userManager, "pgv", "Phòng Giáo vụ", "123456", "PGV");

        // 4. Seed Đơn vị (DonVi) chuẩn hóa
        DonVi? truongUtc = await db.DonVis.FirstOrDefaultAsync(d => d.MaDonVi == "UTC");
        if (truongUtc == null)
        {
            truongUtc = new DonVi
            {
                MaDonVi = "UTC",
                TenDonVi = "Trường Đại học Giao thông Vận tải",
                LoaiDonVi = "TRUONG",
                TrangThai = "ACTIVE",
                Email = "dhgtvt@utc.edu.vn",
                DienThoai = "02437663311",
                CreatedAt = DateTime.UtcNow
            };
            db.DonVis.Add(truongUtc);
            await db.SaveChangesAsync();
        }

        var khoaList = new[]
        {
            new { Ma = "KHOA_CNTT", Ten = "Khoa Công nghệ Thông tin", Email = "cntt@utc.edu.vn", Phone = "02437664422" },
            new { Ma = "KHOA_DTVT", Ten = "Khoa Điện tử - Viễn thông", Email = "dtvt@utc.edu.vn", Phone = "02437664433" },
            new { Ma = "KHOA_QTKD", Ten = "Khoa Quản trị Kinh doanh", Email = "qtkd@utc.edu.vn", Phone = "02437664455" },
            new { Ma = "KHOA_NNA", Ten = "Khoa Ngôn ngữ Anh", Email = "nna@utc.edu.vn", Phone = "02437664466" }
        };

        foreach (var item in khoaList)
        {
            if (!await db.DonVis.AnyAsync(d => d.MaDonVi == item.Ma))
            {
                db.DonVis.Add(new DonVi
                {
                    MaDonVi = item.Ma,
                    TenDonVi = item.Ten,
                    LoaiDonVi = "KHOA",
                    DonViChaId = truongUtc.DonViId,
                    TrangThai = "ACTIVE",
                    Email = item.Email,
                    DienThoai = item.Phone,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await db.SaveChangesAsync();

        var donViCntt = await db.DonVis.FirstOrDefaultAsync(d => d.MaDonVi == "KHOA_CNTT");
        var donViDtvt = await db.DonVis.FirstOrDefaultAsync(d => d.MaDonVi == "KHOA_DTVT");
        var donViQtkd = await db.DonVis.FirstOrDefaultAsync(d => d.MaDonVi == "KHOA_QTKD");
        var donViNna = await db.DonVis.FirstOrDefaultAsync(d => d.MaDonVi == "KHOA_NNA");

        // 5. Seed Ngành & Lớp (gắn DonViId)
        if (!await db.Nganhs.AnyAsync())
        {
            using var trans = await db.Database.BeginTransactionAsync();
            try
            {
                var n1 = new Nganh
                {
                    MaNganh = "CNTT",
                    TenNganh = "Khoa Công nghệ Thông tin",
                    TenKhoa = "Khoa Công nghệ Thông tin",
                    DonViId = donViCntt?.DonViId
                };
                n1.Lops.Add(new Lop { TenLop = "CNTT1-K14" });
                n1.Lops.Add(new Lop { TenLop = "CNTT2-K15" });

                var n2 = new Nganh
                {
                    MaNganh = "DTVT",
                    TenNganh = "Khoa Điện tử - Viễn thông",
                    TenKhoa = "Khoa Điện tử - Viễn thông",
                    DonViId = donViDtvt?.DonViId
                };
                n2.Lops.Add(new Lop { TenLop = "DTVT1-K15" });

                var n3 = new Nganh
                {
                    MaNganh = "QTKD",
                    TenNganh = "Khoa Quản trị Kinh doanh",
                    TenKhoa = "Khoa Quản trị Kinh doanh",
                    DonViId = donViQtkd?.DonViId
                };
                n3.Lops.Add(new Lop { TenLop = "QTKD1-K15" });

                var n4 = new Nganh
                {
                    MaNganh = "NNA",
                    TenNganh = "Khoa Ngôn ngữ Anh",
                    TenKhoa = "Khoa Ngôn ngữ Anh",
                    DonViId = donViNna?.DonViId
                };
                n4.Lops.Add(new Lop { TenLop = "NNA1-K16" });

                db.Nganhs.AddRange(n1, n2, n3, n4);
                await db.SaveChangesAsync();
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }
        else
        {
            // Backfill DonViId nếu ngành chưa có khóa ngoại
            if (donViCntt != null)
            {
                var ngs = await db.Nganhs.Where(n => n.DonViId == null).ToListAsync();
                foreach (var ng in ngs)
                {
                    if (ng.MaNganh == "CNTT") ng.DonViId = donViCntt.DonViId;
                    else if (ng.MaNganh == "DTVT") ng.DonViId = donViDtvt?.DonViId;
                    else if (ng.MaNganh == "QTKD") ng.DonViId = donViQtkd?.DonViId;
                    else if (ng.MaNganh == "NNA") ng.DonViId = donViNna?.DonViId;
                }
                await db.SaveChangesAsync();
            }
        }

        // 6. Seed Giảng viên (gắn DonViId)
        if (giangVienUser != null && !await db.GiangViens.AnyAsync())
        {
            db.GiangViens.Add(new GiangVien
            {
                MaGiangVien = "GV01",
                HoTen = "PGS.TS. Trần Đình Khang",
                Email = "khang.td@eduportal.edu.vn",
                SoDienThoai = "0912345678",
                UserId = giangVienUser.Id,
                DonViId = donViCntt?.DonViId
            });
            await db.SaveChangesAsync();
        }
        else if (donViCntt != null)
        {
            var gvNoDonVi = await db.GiangViens.FirstOrDefaultAsync(g => g.DonViId == null);
            if (gvNoDonVi != null)
            {
                gvNoDonVi.DonViId = donViCntt.DonViId;
                await db.SaveChangesAsync();
            }
        }

        // 7. Seed Danh sách Sinh viên
        if (!await db.SinhViens.AnyAsync())
        {
            using var trans = await db.Database.BeginTransactionAsync();
            try
            {
                var lopCNTT = await db.Lops.FirstOrDefaultAsync(l => l.TenLop.Contains("CNTT")) ?? await db.Lops.FirstOrDefaultAsync();
                var lopDTVT = await db.Lops.FirstOrDefaultAsync(l => l.TenLop.Contains("DTVT")) ?? lopCNTT;
                var lopQTKD = await db.Lops.FirstOrDefaultAsync(l => l.TenLop.Contains("QTKD")) ?? lopCNTT;
                var lopNNA = await db.Lops.FirstOrDefaultAsync(l => l.TenLop.Contains("NNA")) ?? lopCNTT;

                const string trangThaiDangHoc = "Đang học";
                db.SinhViens.AddRange(
                    new SinhVien { MaSinhVien = "SV202401", HoTen = "Nguyễn Văn Hưng", GioiTinh = "Nam", Email = "hung.nv202401@student.edu.vn", SoDienThoai = "0987111222", LopId = lopCNTT?.Id, NgaySinh = new DateTime(2003, 7, 19), UserId = sinhVienUser?.Id },
                    new SinhVien { MaSinhVien = "SV202402", HoTen = "Trần Thị Mai Anh", GioiTinh = "Nữ", Email = "anh.ttm202402@student.edu.vn", SoDienThoai = "0987333444", LopId = lopCNTT?.Id, NgaySinh = new DateTime(2004, 5, 8) },
                    new SinhVien { MaSinhVien = "SV202403", HoTen = "Lê Hoàng Long", GioiTinh = "Nam", Email = "long.lh202403@student.edu.vn", SoDienThoai = "0987555666", LopId = lopQTKD?.Id, NgaySinh = new DateTime(2003, 11, 5) },
                    new SinhVien { MaSinhVien = "SV202404", HoTen = "Phạm Thùy Linh", GioiTinh = "Nữ", Email = "linh.pt202404@student.edu.vn", SoDienThoai = "0987777888", LopId = lopNNA?.Id, NgaySinh = new DateTime(2003, 2, 18) },
                    new SinhVien { MaSinhVien = "SV202405", HoTen = "Đỗ Quốc Bảo", GioiTinh = "Nam", Email = "bao.dq202405@student.edu.vn", SoDienThoai = "0987999000", LopId = lopDTVT?.Id, NgaySinh = new DateTime(2003, 7, 19) },
                    new SinhVien { MaSinhVien = "SV202406", HoTen = "Bùi Phương Thảo", GioiTinh = "Nữ", Email = "thao.bp202406@student.edu.vn", SoDienThoai = "0912111333", LopId = lopNNA?.Id, NgaySinh = new DateTime(2004, 5, 8) },
                    new SinhVien { MaSinhVien = "SV202407", HoTen = "Ngô Minh Tuấn", GioiTinh = "Nam", Email = "tuan.nm202407@student.edu.vn", SoDienThoai = "0912444555", LopId = lopCNTT?.Id, NgaySinh = new DateTime(2002, 3, 25) },
                    new SinhVien { MaSinhVien = "SV202408", HoTen = "Đặng Quang Huy", GioiTinh = "Nam", Email = "huy.dq202408@student.edu.vn", SoDienThoai = "0912777888", LopId = lopCNTT?.Id, NgaySinh = new DateTime(2003, 12, 14) },
                    new SinhVien { MaSinhVien = "SV202409", HoTen = "Hoàng Ngọc Ánh", GioiTinh = "Nữ", Email = "anh.hn202409@student.edu.vn", SoDienThoai = "0912999111", LopId = lopQTKD?.Id, NgaySinh = new DateTime(2003, 6, 30) },
                    new SinhVien { MaSinhVien = "SV202410", HoTen = "Vũ Đình Trọng", GioiTinh = "Nam", Email = "trong.vd202410@student.edu.vn", SoDienThoai = "0912222444", LopId = lopDTVT?.Id, NgaySinh = new DateTime(2002, 10, 12) }
                );

                foreach (var sinhVien in db.SinhViens)
                {
                    sinhVien.TrangThai = trangThaiDangHoc;
                    sinhVien.NienKhoa = sinhVien.NgaySinh?.Year.ToString();
                }

                await db.SaveChangesAsync();
                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }
        }

        // 8. Seed Môn học (gắn DonViId)
        if (!await db.MonHocs.AnyAsync())
        {
            db.MonHocs.AddRange(
                new MonHoc { MaMon = "INT301", TenMon = "Lập trình Web ASP.NET Core MVC", SoTinChi = 3, DonViId = donViCntt?.DonViId },
                new MonHoc { MaMon = "INT201", TenMon = "Cấu trúc dữ liệu và giải thuật", SoTinChi = 4, DonViId = donViCntt?.DonViId },
                new MonHoc { MaMon = "INT205", TenMon = "Hệ quản trị cơ sở dữ liệu SQL Server", SoTinChi = 3, DonViId = donViCntt?.DonViId },
                new MonHoc { MaMon = "BUS101", TenMon = "Kinh tế vi mô", SoTinChi = 3, DonViId = donViQtkd?.DonViId },
                new MonHoc { MaMon = "BUS202", TenMon = "Marketing căn bản", SoTinChi = 3, DonViId = donViQtkd?.DonViId },
                new MonHoc { MaMon = "ENG201", TenMon = "Tiếng Anh chuyên ngành CNTT", SoTinChi = 3, DonViId = donViNna?.DonViId }
            );
            await db.SaveChangesAsync();
        }
        else if (donViCntt != null)
        {
            var mhList = await db.MonHocs.Where(m => m.DonViId == null).ToListAsync();
            foreach (var mh in mhList)
            {
                if (mh.MaMon.StartsWith("INT")) mh.DonViId = donViCntt.DonViId;
                else if (mh.MaMon.StartsWith("BUS")) mh.DonViId = donViQtkd?.DonViId;
                else if (mh.MaMon.StartsWith("ENG")) mh.DonViId = donViNna?.DonViId;
            }
            await db.SaveChangesAsync();
        }

        // 9. Seed 9 Kết quả điểm học phần
        if (!await db.Diems.AnyAsync())
        {
            var sv1 = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Hưng"));
            var sv2 = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Mai Anh"));
            var sv3 = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Long"));
            var sv4 = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Linh"));

            var mINT301 = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT301");
            var mINT201 = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT201");
            var mINT205 = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT205");
            var mBUS101 = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "BUS101");
            var mBUS202 = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "BUS202");
            var mENG201 = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "ENG201");

            if (sv1 != null && mINT301 != null && mINT201 != null && mINT205 != null)
            {
                db.Diems.Add(new Diem { SinhVienId = sv1.Id, MonHocId = mINT301.Id, DiemChuyenCan = 10, DiemGiuaKy = 9.5, DiemCuoiKy = 9.0 });
                db.Diems.Add(new Diem { SinhVienId = sv1.Id, MonHocId = mINT201.Id, DiemChuyenCan = 9.5, DiemGiuaKy = 9.0, DiemCuoiKy = 9.2 });
                db.Diems.Add(new Diem { SinhVienId = sv1.Id, MonHocId = mINT205.Id, DiemChuyenCan = 10, DiemGiuaKy = 9.0, DiemCuoiKy = 9.5 });
            }

            if (sv2 != null && mINT301 != null && mINT201 != null)
            {
                db.Diems.Add(new Diem { SinhVienId = sv2.Id, MonHocId = mINT301.Id, DiemChuyenCan = 10, DiemGiuaKy = 8.5, DiemCuoiKy = 8.8 });
                db.Diems.Add(new Diem { SinhVienId = sv2.Id, MonHocId = mINT201.Id, DiemChuyenCan = 9.0, DiemGiuaKy = 8.0, DiemCuoiKy = 9.0 });
            }

            if (sv3 != null && mBUS101 != null && mBUS202 != null)
            {
                db.Diems.Add(new Diem { SinhVienId = sv3.Id, MonHocId = mBUS101.Id, DiemChuyenCan = 9.0, DiemGiuaKy = 8.0, DiemCuoiKy = 8.0 });
                db.Diems.Add(new Diem { SinhVienId = sv3.Id, MonHocId = mBUS202.Id, DiemChuyenCan = 8.5, DiemGiuaKy = 8.0, DiemCuoiKy = 8.2 });
            }

            if (sv4 != null && mENG201 != null && mBUS101 != null)
            {
                db.Diems.Add(new Diem { SinhVienId = sv4.Id, MonHocId = mENG201.Id, DiemChuyenCan = 10, DiemGiuaKy = 9.5, DiemCuoiKy = 9.5 });
                db.Diems.Add(new Diem { SinhVienId = sv4.Id, MonHocId = mBUS101.Id, DiemChuyenCan = 10, DiemGiuaKy = 9.0, DiemCuoiKy = 9.2 });
            }

            await db.SaveChangesAsync();
        }

        // 10. Seed Lịch giảng dạy
        if (!await db.LichGiangDays.AnyAsync())
        {
            var gv = await db.GiangViens.FirstOrDefaultAsync();
            var lop1 = await db.Lops.FirstOrDefaultAsync(l => l.TenLop.Contains("CNTT1")) ?? await db.Lops.FirstOrDefaultAsync();
            var lop2 = await db.Lops.FirstOrDefaultAsync(l => l.TenLop.Contains("CNTT2")) ?? lop1;
            var mWeb = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT301") ?? await db.MonHocs.FirstOrDefaultAsync();
            var mCtdl = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT201") ?? mWeb;
            var mSql = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT205") ?? mWeb;

            if (mWeb != null && lop1 != null)
            {
                db.LichGiangDays.AddRange(
                    new LichGiangDay
                    {
                        GiangVienId = gv?.Id,
                        MonHocId = mWeb.Id,
                        LopId = lop1.Id,
                        Thu = "Thứ 2",
                        TietBatDau = 1,
                        SoTiet = 3,
                        PhongHoc = "P.302-A1",
                        GhiChu = "Lý thuyết & Thảo luận",
                        HocKy = "Học kỳ 1 (2026 - 2027)"
                    },
                    new LichGiangDay
                    {
                        GiangVienId = gv?.Id,
                        MonHocId = mCtdl != null ? mCtdl.Id : mWeb.Id,
                        LopId = lop2 != null ? lop2.Id : lop1.Id,
                        Thu = "Thứ 3",
                        TietBatDau = 7,
                        SoTiet = 4,
                        PhongHoc = "Lab 4-B2",
                        GhiChu = "Thực hành trên máy tính",
                        HocKy = "Học kỳ 1 (2026 - 2027)"
                    },
                    new LichGiangDay
                    {
                        GiangVienId = gv?.Id,
                        MonHocId = mSql != null ? mSql.Id : mWeb.Id,
                        LopId = lop1.Id,
                        Thu = "Thứ 5",
                        TietBatDau = 2,
                        SoTiet = 3,
                        PhongHoc = "P.205-A2",
                        GhiChu = "Bài giảng môn chuyên ngành",
                        HocKy = "Học kỳ 1 (2026 - 2027)"
                    },
                    new LichGiangDay
                    {
                        GiangVienId = gv?.Id,
                        MonHocId = mWeb.Id,
                        LopId = lop2 != null ? lop2.Id : lop1.Id,
                        Thu = "Thứ 6",
                        TietBatDau = 6,
                        SoTiet = 3,
                        PhongHoc = "P.401-A1",
                        GhiChu = "Kiểm tra định kỳ & Báo cáo BTL",
                        HocKy = "Học kỳ 1 (2026 - 2027)"
                    }
                );
                await db.SaveChangesAsync();
            }
        }

        // 11. Seed Điểm danh mẫu
        if (!await db.DiemDanhs.AnyAsync())
        {
            var lop1 = await db.Lops.FirstOrDefaultAsync(l => l.TenLop.Contains("CNTT1")) ?? await db.Lops.FirstOrDefaultAsync();
            var mWeb = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT301") ?? await db.MonHocs.FirstOrDefaultAsync();
            if (lop1 != null && mWeb != null)
            {
                var svs = await db.SinhViens.Where(s => s.LopId == lop1.Id).Take(5).ToListAsync();
                int idx = 0;
                foreach (var sv in svs)
                {
                    idx++;
                    db.DiemDanhs.Add(new DiemDanh
                    {
                        SinhVienId = sv.Id,
                        LopId = lop1.Id,
                        MonHocId = mWeb.Id,
                        NgayDiemDanh = DateTime.Today,
                        TrangThai = (idx == 3) ? "Vắng" : (idx == 4 ? "Có phép" : "Có mặt"),
                        GhiChu = (idx == 4) ? "Có đơn xin phép nghỉ ốm" : null
                    });
                }
                await db.SaveChangesAsync();
            }
        }
    }

    private static async Task<ApplicationUser?> CreateUser(
        UserManager<ApplicationUser> userManager,
        string userName,
        string hoTen,
        string password,
        string role)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user != null)
        {
            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Không thể bật khóa đăng nhập cho tài khoản mẫu {userName}: " +
                        string.Join("; ", updateResult.Errors.Select(error => error.Description)));
                }
            }

            return user;
        }

        user = new ApplicationUser
        {
            UserName = userName,
            HoTen = hoTen,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return null;
        }

        await userManager.AddToRoleAsync(user, role);
        return user;
    }
}
