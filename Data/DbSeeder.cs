using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        // Tự động đảm bảo tạo bảng DiemDanhs và LichGiangDays nếu chưa tồn tại
        await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DiemDanhs' and xtype='U')
            BEGIN
                CREATE TABLE DiemDanhs (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    SinhVienId INT NOT NULL,
                    LopId INT NOT NULL,
                    MonHocId INT NOT NULL,
                    NgayDiemDanh DATETIME2 NOT NULL,
                    TrangThai NVARCHAR(50) NOT NULL,
                    GhiChu NVARCHAR(255) NULL
                );
            END;

            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LichGiangDays' and xtype='U')
            BEGIN
                CREATE TABLE LichGiangDays (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    GiangVienId INT NULL,
                    MonHocId INT NOT NULL,
                    LopId INT NOT NULL,
                    Thu NVARCHAR(20) NOT NULL,
                    TietBatDau INT NOT NULL,
                    SoTiet INT NOT NULL,
                    PhongHoc NVARCHAR(50) NOT NULL,
                    GhiChu NVARCHAR(255) NULL,
                    HocKy NVARCHAR(100) NOT NULL
                );
            END;
        ");

        // Seed Roles
        foreach (var role in new[] { "Admin", "GiangVien", "SinhVien", "PGV" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Users
        var adminUser = await CreateUser(userManager, "admin", "Quản trị viên", "Admin@123", "Admin");
        var giangVienUser = await CreateUser(userManager, "gv01", "PGS.TS. Trần Đình Khang", "GiangVien@123", "GiangVien");
        var sinhVienUser = await CreateUser(userManager, "sv01", "Nguyễn Văn Hưng", "SinhVien@123", "SinhVien");
        await CreateUser(userManager, "pgv", "Phòng Giáo vụ", "123456", "PGV");

        // Seed Ngành & Lớp
        if (!db.Nganhs.Any())
        {
            var n1 = new Nganh { MaNganh = "CNTT", TenNganh = "Khoa Công nghệ Thông tin" };
            n1.Lops.Add(new Lop { TenLop = "CNTT1-K14" });
            n1.Lops.Add(new Lop { TenLop = "CNTT2-K15" });

            var n2 = new Nganh { MaNganh = "DTVT", TenNganh = "Khoa Điện tử - Viễn thông" };
            n2.Lops.Add(new Lop { TenLop = "DTVT1-K15" });

            var n3 = new Nganh { MaNganh = "QTKD", TenNganh = "Khoa Quản trị Kinh doanh" };
            n3.Lops.Add(new Lop { TenLop = "QTKD1-K15" });

            var n4 = new Nganh { MaNganh = "NNA", TenNganh = "Khoa Ngôn ngữ Anh" };
            n4.Lops.Add(new Lop { TenLop = "NNA1-K16" });

            db.Nganhs.AddRange(n1, n2, n3, n4);
            await db.SaveChangesAsync();
        }

        // Seed Giảng viên
        if (giangVienUser != null && !db.GiangViens.Any())
        {
            db.GiangViens.Add(new GiangVien
            {
                MaGiangVien = "GV01",
                HoTen = "PGS.TS. Trần Đình Khang",
                Email = "khang.td@eduportal.edu.vn",
                SoDienThoai = "0912345678",
                UserId = giangVienUser.Id
            });
            await db.SaveChangesAsync();
        }

        // Seed Danh sách Sinh viên
        if (!db.SinhViens.Any())
        {
            var lopCNTT = db.Lops.FirstOrDefault(l => l.TenLop.Contains("CNTT")) ?? db.Lops.FirstOrDefault();
            var lopDTVT = db.Lops.FirstOrDefault(l => l.TenLop.Contains("DTVT")) ?? lopCNTT;
            var lopQTKD = db.Lops.FirstOrDefault(l => l.TenLop.Contains("QTKD")) ?? lopCNTT;
            var lopNNA = db.Lops.FirstOrDefault(l => l.TenLop.Contains("NNA")) ?? lopCNTT;

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
        }

        // Seed Môn học / Học phần
        if (!db.MonHocs.Any())
        {
            db.MonHocs.AddRange(
                new MonHoc { MaMon = "INT301", TenMon = "Lập trình Web ASP.NET Core MVC", SoTinChi = 3 },
                new MonHoc { MaMon = "INT201", TenMon = "Cấu trúc dữ liệu và giải thuật", SoTinChi = 4 },
                new MonHoc { MaMon = "INT205", TenMon = "Hệ quản trị cơ sở dữ liệu SQL Server", SoTinChi = 3 },
                new MonHoc { MaMon = "BUS101", TenMon = "Kinh tế vi mô", SoTinChi = 3 },
                new MonHoc { MaMon = "BUS202", TenMon = "Marketing căn bản", SoTinChi = 3 },
                new MonHoc { MaMon = "ENG201", TenMon = "Tiếng Anh chuyên ngành CNTT", SoTinChi = 3 }
            );
            await db.SaveChangesAsync();
        }

        // Seed 9 Kết quả điểm học phần như trong Ảnh mẫu số 3
        if (!db.Diems.Any())
        {
            var sv1 = db.SinhViens.FirstOrDefault(s => s.HoTen.Contains("Hưng"));
            var sv2 = db.SinhViens.FirstOrDefault(s => s.HoTen.Contains("Mai Anh"));
            var sv3 = db.SinhViens.FirstOrDefault(s => s.HoTen.Contains("Long"));
            var sv4 = db.SinhViens.FirstOrDefault(s => s.HoTen.Contains("Linh"));

            var mINT301 = db.MonHocs.FirstOrDefault(m => m.MaMon == "INT301");
            var mINT201 = db.MonHocs.FirstOrDefault(m => m.MaMon == "INT201");
            var mINT205 = db.MonHocs.FirstOrDefault(m => m.MaMon == "INT205");
            var mBUS101 = db.MonHocs.FirstOrDefault(m => m.MaMon == "BUS101");
            var mBUS202 = db.MonHocs.FirstOrDefault(m => m.MaMon == "BUS202");
            var mENG201 = db.MonHocs.FirstOrDefault(m => m.MaMon == "ENG201");

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

        // Seed Thời khóa biểu / Lịch giảng dạy của giảng viên
        if (!db.LichGiangDays.Any())
        {
            var gv = db.GiangViens.FirstOrDefault();
            var lop1 = db.Lops.FirstOrDefault(l => l.TenLop.Contains("CNTT1")) ?? db.Lops.FirstOrDefault();
            var lop2 = db.Lops.FirstOrDefault(l => l.TenLop.Contains("CNTT2")) ?? lop1;
            var mWeb = db.MonHocs.FirstOrDefault(m => m.MaMon == "INT301") ?? db.MonHocs.FirstOrDefault();
            var mCtdl = db.MonHocs.FirstOrDefault(m => m.MaMon == "INT201") ?? mWeb;
            var mSql = db.MonHocs.FirstOrDefault(m => m.MaMon == "INT205") ?? mWeb;

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

        // Seed Mẫu dữ liệu điểm danh
        if (!db.DiemDanhs.Any())
        {
            var lop1 = db.Lops.FirstOrDefault(l => l.TenLop.Contains("CNTT1")) ?? db.Lops.FirstOrDefault();
            var mWeb = db.MonHocs.FirstOrDefault(m => m.MaMon == "INT301") ?? db.MonHocs.FirstOrDefault();
            if (lop1 != null && mWeb != null)
            {
                var svs = db.SinhViens.Where(s => s.LopId == lop1.Id).Take(5).ToList();
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
