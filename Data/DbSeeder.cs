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
            logger.LogWarning(ex, "Không thể tự động áp dụng EF Migration qua MigrateAsync (có thể database đã có sẵn cấu trúc hoặc đang kiểm tra thủ công). Tiếp tục seed dữ liệu cần thiết.");
        }

        // Đảm bảo các bảng Thanh toán & VietQR luôn tồn tại an toàn (Self-Healing Schema)
        await EnsurePaymentTablesAsync(db, logger);

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

        // 12. Seed Năm học & Học kỳ
        var namHoc2026 = await db.NamHocs.FirstOrDefaultAsync(n => n.MaNamHoc == "2026-2027");
        if (namHoc2026 == null)
        {
            namHoc2026 = new NamHoc
            {
                MaNamHoc = "2026-2027",
                NgayBatDau = new DateOnly(2026, 8, 15),
                NgayKetThuc = new DateOnly(2027, 6, 30),
                TrangThai = "DANG_MO"
            };
            db.NamHocs.Add(namHoc2026);
            await db.SaveChangesAsync();
        }

        var hocKy1 = await db.HocKies.FirstOrDefaultAsync(h => h.MaHocKy == "HK1-2026");
        if (hocKy1 == null)
        {
            hocKy1 = new HocKy
            {
                NamHocId = namHoc2026.NamHocId,
                MaHocKy = "HK1-2026",
                TenHocKy = "Học kỳ 1 (2026 - 2027)",
                NgayBatDau = new DateOnly(2026, 8, 15),
                NgayKetThuc = new DateOnly(2027, 1, 15),
                HanDangKyTu = new DateOnly(2026, 8, 1),
                HanDangKyDen = new DateOnly(2026, 8, 25),
                TrangThai = "DANG_MO"
            };
            db.HocKies.Add(hocKy1);
            await db.SaveChangesAsync();
        }

        // 13. Seed Lớp học phần (LopHocPhan)
        var mINT301Entity = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT301");
        var mINT201Entity = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT201");
        var mINT205Entity = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "INT205");
        var mBUS101Entity = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "BUS101");
        var mENG201Entity = await db.MonHocs.FirstOrDefaultAsync(m => m.MaMon == "ENG201");
        var gvKhang = await db.GiangViens.FirstOrDefaultAsync();

        if (mINT301Entity != null && !await db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == "INT301.01"))
        {
            db.LopHocPhans.Add(new LopHocPhan
            {
                MaLopHocPhan = "INT301.01",
                TenLopHocPhan = "Lập trình Web ASP.NET Core MVC - Nhóm 01",
                MonHocId = mINT301Entity.Id,
                HocKyId = hocKy1.HocKyId,
                GiangVienId = gvKhang?.Id,
                Nhom = "01",
                SiSoToiDa = 60,
                TrangThai = "OPEN",
                Thu = "Thứ 4",
                TietBatDau = 1,
                SoTiet = 3,
                PhongHoc = "P.302-A2"
            });
        }

        if (mINT201Entity != null && !await db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == "INT201.01"))
        {
            db.LopHocPhans.Add(new LopHocPhan
            {
                MaLopHocPhan = "INT201.01",
                TenLopHocPhan = "Cấu trúc dữ liệu và giải thuật - Nhóm 01",
                MonHocId = mINT201Entity.Id,
                HocKyId = hocKy1.HocKyId,
                GiangVienId = gvKhang?.Id,
                Nhom = "01",
                SiSoToiDa = 60,
                TrangThai = "OPEN",
                Thu = "Thứ 2",
                TietBatDau = 1,
                SoTiet = 4,
                PhongHoc = "P.205-A2"
            });
        }

        if (mINT205Entity != null && !await db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == "INT205.01"))
        {
            db.LopHocPhans.Add(new LopHocPhan
            {
                MaLopHocPhan = "INT205.01",
                TenLopHocPhan = "Hệ quản trị CSDL SQL Server - Nhóm 01",
                MonHocId = mINT205Entity.Id,
                HocKyId = hocKy1.HocKyId,
                GiangVienId = gvKhang?.Id,
                Nhom = "01",
                SiSoToiDa = 50,
                TrangThai = "OPEN",
                Thu = "Thứ 3",
                TietBatDau = 7,
                SoTiet = 3,
                PhongHoc = "P.401-A1"
            });
        }

        if (mBUS101Entity != null && !await db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == "BUS101.01"))
        {
            db.LopHocPhans.Add(new LopHocPhan
            {
                MaLopHocPhan = "BUS101.01",
                TenLopHocPhan = "Kinh tế vi mô - Nhóm 01",
                MonHocId = mBUS101Entity.Id,
                HocKyId = hocKy1.HocKyId,
                GiangVienId = gvKhang?.Id,
                Nhom = "01",
                SiSoToiDa = 70,
                TrangThai = "OPEN",
                Thu = "Thứ 3",
                TietBatDau = 1,
                SoTiet = 3,
                PhongHoc = "P.102-A1"
            });
        }

        if (mENG201Entity != null && !await db.LopHocPhans.AnyAsync(l => l.MaLopHocPhan == "ENG201.01"))
        {
            db.LopHocPhans.Add(new LopHocPhan
            {
                MaLopHocPhan = "ENG201.01",
                TenLopHocPhan = "Tiếng Anh chuyên ngành CNTT - Nhóm 01",
                MonHocId = mENG201Entity.Id,
                HocKyId = hocKy1.HocKyId,
                GiangVienId = gvKhang?.Id,
                Nhom = "01",
                SiSoToiDa = 50,
                TrangThai = "OPEN",
                Thu = "Thứ 6",
                TietBatDau = 7,
                SoTiet = 3,
                PhongHoc = "P.201-A1"
            });
        }
        await db.SaveChangesAsync();

        // 14. Seed Đăng ký học phần (DangKyHocPhan) - Sinh viên từ nhiều lớp hành chính cùng đăng ký
        var lhpWeb01 = await db.LopHocPhans.FirstOrDefaultAsync(l => l.MaLopHocPhan == "INT301.01");
        var lhpCtdl01 = await db.LopHocPhans.FirstOrDefaultAsync(l => l.MaLopHocPhan == "INT201.01");
        var lhpSql01 = await db.LopHocPhans.FirstOrDefaultAsync(l => l.MaLopHocPhan == "INT205.01");
        var lhpKtvm01 = await db.LopHocPhans.FirstOrDefaultAsync(l => l.MaLopHocPhan == "BUS101.01");

        var svHung = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Hưng"));
        var svMaiAnh = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Mai Anh"));
        var svLong = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Long"));
        var svLinh = await db.SinhViens.FirstOrDefaultAsync(s => s.HoTen.Contains("Linh"));

        var enrollments = new List<(SinhVien? sv, LopHocPhan? lhp)>
        {
            (svHung, lhpWeb01),
            (svHung, lhpCtdl01),
            (svHung, lhpSql01),
            (svMaiAnh, lhpWeb01),
            (svMaiAnh, lhpCtdl01),
            (svLong, lhpWeb01), // Long thuộc lớp QTKD nhưng học chung lớp HP Lập trình Web với Hưng (CNTT)
            (svLong, lhpKtvm01),
            (svLinh, lhpKtvm01)
        };

        foreach (var (sv, lhp) in enrollments)
        {
            if (sv != null && lhp != null && !await db.DangKyHocPhans.AnyAsync(d => d.SinhVienId == sv.Id && d.LopHocPhanId == lhp.Id))
            {
                db.DangKyHocPhans.Add(new DangKyHocPhan
                {
                    SinhVienId = sv.Id,
                    LopHocPhanId = lhp.Id,
                    NgayDangKy = DateTime.UtcNow,
                    TrangThai = "ENROLLED",
                    LanHoc = 1
                });
            }
        }
        await db.SaveChangesAsync();

        // 15. Cập nhật LopHocPhanId cho Điểm và Điểm danh (chỉ gán nếu sinh viên chưa có bản ghi trong LHP đó)
        if (lhpWeb01 != null && mINT301Entity != null)
        {
            var diemsWeb = await db.Diems.Where(d => d.MonHocId == mINT301Entity.Id && d.LopHocPhanId == null).ToListAsync();
            foreach (var d in diemsWeb)
            {
                if (!await db.Diems.AnyAsync(x => x.SinhVienId == d.SinhVienId && x.LopHocPhanId == lhpWeb01.Id))
                {
                    d.LopHocPhanId = lhpWeb01.Id;
                }
            }

            var ddWeb = await db.DiemDanhs.Where(d => d.MonHocId == mINT301Entity.Id && d.LopHocPhanId == null).ToListAsync();
            foreach (var dd in ddWeb) dd.LopHocPhanId = lhpWeb01.Id;
            await db.SaveChangesAsync();
        }

        if (lhpCtdl01 != null && mINT201Entity != null)
        {
            var diemsCtdl = await db.Diems.Where(d => d.MonHocId == mINT201Entity.Id && d.LopHocPhanId == null).ToListAsync();
            foreach (var d in diemsCtdl)
            {
                if (!await db.Diems.AnyAsync(x => x.SinhVienId == d.SinhVienId && x.LopHocPhanId == lhpCtdl01.Id))
                {
                    d.LopHocPhanId = lhpCtdl01.Id;
                }
            }
            await db.SaveChangesAsync();
        }

        if (lhpSql01 != null && mINT205Entity != null)
        {
            var diemsSql = await db.Diems.Where(d => d.MonHocId == mINT205Entity.Id && d.LopHocPhanId == null).ToListAsync();
            foreach (var d in diemsSql)
            {
                if (!await db.Diems.AnyAsync(x => x.SinhVienId == d.SinhVienId && x.LopHocPhanId == lhpSql01.Id))
                {
                    d.LopHocPhanId = lhpSql01.Id;
                }
            }
            await db.SaveChangesAsync();
        }

        if (lhpKtvm01 != null && mBUS101Entity != null)
        {
            var diemsKtvm = await db.Diems.Where(d => d.MonHocId == mBUS101Entity.Id && d.LopHocPhanId == null).ToListAsync();
            foreach (var d in diemsKtvm)
            {
                if (!await db.Diems.AnyAsync(x => x.SinhVienId == d.SinhVienId && x.LopHocPhanId == lhpKtvm01.Id))
                {
                    d.LopHocPhanId = lhpKtvm01.Id;
                }
            }
            await db.SaveChangesAsync();
        }

        // 16. Chuẩn hóa & Seed dữ liệu mô phỏng nghiệp vụ Học lại, Học cải thiện, Thi lại
        // a) Cập nhật các bản ghi hiện tại
        var currentDiems = await db.Diems.ToListAsync();
        foreach (var d in currentDiems)
        {
            if (d.HocKyId == null) d.HocKyId = hocKy1.HocKyId;
            if (string.IsNullOrEmpty(d.LoaiHoc)) d.LoaiHoc = "HOC_LAN_DAU";
            d.TrangThai = d.DiemTongKet >= 4.0 ? "DAT" : "KHONG_DAT";
        }
        await db.SaveChangesAsync();

        // b) Kịch bản 1: SV202401 (Nguyễn Văn Hưng) từng trượt môn INT201 lần 1, sau đó học lại đạt 9.2
        if (svHung != null && mINT201Entity != null)
        {
            var diemHungHocLai = await db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == svHung.Id && d.MonHocId == mINT201Entity.Id && d.LanHoc == 2);
            var diemHungLan1 = await db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == svHung.Id && d.MonHocId == mINT201Entity.Id && d.LanHoc == 1);

            if (diemHungLan1 != null && diemHungHocLai == null)
            {
                // Chuyển bản ghi đạt hiện tại thành lần 2 (Học lại)
                diemHungLan1.LoaiHoc = "HOC_LAI";
                diemHungLan1.LanHoc = 2;
                diemHungLan1.LanThi = 2;
                diemHungLan1.SuDungTinhTichLuy = true;
                diemHungLan1.GhiChu = "Học lại lần 2 do lần 1 không đạt (F)";

                // Thêm bản ghi lịch sử lần 1 (bị F)
                db.Diems.Add(new Diem
                {
                    SinhVienId = svHung.Id,
                    MonHocId = mINT201Entity.Id,
                    HocKyId = hocKy1.HocKyId,
                    LoaiHoc = "HOC_LAN_DAU",
                    LanHoc = 1,
                    LanThi = 1,
                    DiemChuyenCan = 4.0,
                    DiemGiuaKy = 3.0,
                    DiemCuoiKy = 3.0,
                    TrangThai = "KHONG_DAT",
                    SuDungTinhTichLuy = false,
                    GhiChu = "Học lần đầu không đạt (F = 3.1 điểm)",
                    NgayCapNhat = DateTime.UtcNow.AddMonths(-5),
                    NguoiCapNhat = "HeThong"
                });
                await db.SaveChangesAsync();
            }
        }

        // c) Kịch bản 2: SV202403 (Lê Hoàng Long) đã đạt BUS101 lần 1 (4.8 điểm - D+), học cải thiện lần 2 đạt 8.0 (B+)
        if (svLong != null && mBUS101Entity != null)
        {
            var diemLongCaiThien = await db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == svLong.Id && d.MonHocId == mBUS101Entity.Id && d.LanHoc == 2);
            var diemLongLan1 = await db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == svLong.Id && d.MonHocId == mBUS101Entity.Id && d.LanHoc == 1);

            if (diemLongLan1 != null && diemLongCaiThien == null)
            {
                // Bản ghi hiện tại đang là 8.0, chuyển thành lần 2 (Cải thiện)
                diemLongLan1.LoaiHoc = "HOC_CAI_THIEN";
                diemLongLan1.LanHoc = 2;
                diemLongLan1.LanThi = 2;
                diemLongLan1.SuDungTinhTichLuy = true;
                diemLongLan1.GhiChu = "Học cải thiện điểm (lần 1 đạt D+ 4.7 điểm)";

                // Thêm bản ghi lịch sử lần 1
                db.Diems.Add(new Diem
                {
                    SinhVienId = svLong.Id,
                    MonHocId = mBUS101Entity.Id,
                    HocKyId = hocKy1.HocKyId,
                    LoaiHoc = "HOC_LAN_DAU",
                    LanHoc = 1,
                    LanThi = 1,
                    DiemChuyenCan = 5.0,
                    DiemGiuaKy = 5.0,
                    DiemCuoiKy = 4.5,
                    TrangThai = "DAT",
                    SuDungTinhTichLuy = false, // Không tính vì đã có lần học cải thiện đạt 8.0 cao hơn
                    GhiChu = "Học lần đầu đạt D+ (4.7 điểm)",
                    NgayCapNhat = DateTime.UtcNow.AddMonths(-5),
                    NguoiCapNhat = "HeThong"
                });
                await db.SaveChangesAsync();
            }
        }

        // d) Kịch bản 3: SV202402 (Trần Thị Mai Anh) môn INT301 thi lần 1 bị điểm 3.0, thi lại lần 2 đạt 8.8
        if (svMaiAnh != null && mINT301Entity != null)
        {
            var diemMaiAnhWeb = await db.Diems.FirstOrDefaultAsync(d => d.SinhVienId == svMaiAnh.Id && d.MonHocId == mINT301Entity.Id);
            if (diemMaiAnhWeb != null && diemMaiAnhWeb.LanThi == 1 && !diemMaiAnhWeb.DiemCuoiKyLan2.HasValue)
            {
                diemMaiAnhWeb.LanThi = 2;
                diemMaiAnhWeb.DiemCuoiKy = 3.0; // Thi lần 1
                diemMaiAnhWeb.DiemCuoiKyLan2 = 8.8; // Thi lại lần 2 đạt điểm cao
                diemMaiAnhWeb.GhiChu = "Thi lại kết thúc học phần lần 2 (L1: 3.0, L2: 8.8)";
                diemMaiAnhWeb.SuDungTinhTichLuy = true;
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

    public static async Task EnsurePaymentTablesAsync(ApplicationDbContext db, ILogger logger)
    {
        try
        {
            var sql = @"
IF OBJECT_ID(N'[HoaDonHocPhi]', N'U') IS NULL
BEGIN
    CREATE TABLE [HoaDonHocPhi] (
        [Id] bigint NOT NULL IDENTITY(1,1),
        [SoHoaDon] nvarchar(50) NOT NULL,
        [SinhVienId] int NOT NULL,
        [HocKyId] int NOT NULL,
        [TenKhoanThu] nvarchar(200) NOT NULL,
        [SoTien] decimal(18,2) NOT NULL,
        [DaThanhToan] decimal(18,2) NOT NULL DEFAULT 0,
        [NgayLap] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        [HanThanhToan] date NULL,
        [TrangThai] nvarchar(30) NOT NULL DEFAULT 'CHUA_THANH_TOAN',
        [GhiChu] nvarchar(500) NULL,
        [DotThuId] int NULL,
        CONSTRAINT [PK_HoaDonHocPhi] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_HoaDonHocPhi_SoHoaDon] ON [HoaDonHocPhi] ([SoHoaDon]);
    CREATE INDEX [IX_HoaDonHocPhi_SinhVienId_HocKyId] ON [HoaDonHocPhi] ([SinhVienId], [HocKyId]);
END;

IF COL_LENGTH(N'[HoaDonHocPhi]', N'DotThuId') IS NULL
BEGIN
    ALTER TABLE [HoaDonHocPhi] ADD [DotThuId] int NULL;
END;

IF OBJECT_ID(N'[VietQrConfig]', N'U') IS NULL
BEGIN
    CREATE TABLE [VietQrConfig] (
        [Id] int NOT NULL IDENTITY(1,1),
        [BankId] nvarchar(20) NOT NULL,
        [BankName] nvarchar(100) NOT NULL,
        [AccountNumber] nvarchar(50) NOT NULL,
        [AccountHolderName] nvarchar(100) NOT NULL,
        [Template] nvarchar(20) NOT NULL DEFAULT 'compact2',
        [ApiBaseUrl] nvarchar(255) NOT NULL,
        [ClientId] nvarchar(100) NULL,
        [ApiKey] nvarchar(100) NULL,
        [WebhookSecret] nvarchar(100) NOT NULL,
        [Environment] nvarchar(20) NOT NULL DEFAULT 'Sandbox',
        [UpdatedAt] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        CONSTRAINT [PK_VietQrConfig] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'[Payment]', N'U') IS NULL
BEGIN
    CREATE TABLE [Payment] (
        [Id] bigint NOT NULL IDENTITY(1,1),
        [TransactionCode] nvarchar(50) NOT NULL,
        [HoaDonHocPhiId] bigint NOT NULL,
        [SinhVienId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [PaymentContent] nvarchar(100) NOT NULL,
        [PaymentMethod] nvarchar(50) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        [ExpiredAt] datetime2 NOT NULL,
        [PaidAt] datetime2 NULL,
        [BankTransactionId] nvarchar(100) NULL,
        [BankReference] nvarchar(100) NULL,
        [QrUrl] nvarchar(500) NULL,
        [RawWebhookData] nvarchar(max) NULL,
        [IsAutoVerified] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Payment] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_Payment_TransactionCode] ON [Payment] ([TransactionCode]);
    CREATE INDEX [IX_Payment_HoaDonHocPhiId] ON [Payment] ([HoaDonHocPhiId]);
    CREATE INDEX [IX_Payment_SinhVienId] ON [Payment] ([SinhVienId]);
    CREATE INDEX [IX_Payment_Status] ON [Payment] ([Status]);
END;

IF OBJECT_ID(N'[BankTransaction]', N'U') IS NULL
BEGIN
    CREATE TABLE [BankTransaction] (
        [Id] bigint NOT NULL IDENTITY(1,1),
        [BankTransactionId] nvarchar(100) NOT NULL,
        [TransactionId] nvarchar(50) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [TransactionContent] nvarchar(255) NOT NULL,
        [TransactionDate] datetime2 NOT NULL,
        [Direction] nvarchar(10) NOT NULL DEFAULT 'IN',
        [PaymentId] bigint NULL,
        [ProcessedAt] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        [RawData] nvarchar(max) NULL,
        CONSTRAINT [PK_BankTransaction] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_BankTransaction_BankTransactionId] ON [BankTransaction] ([BankTransactionId]);
END;

IF OBJECT_ID(N'[PaymentWebhookLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [PaymentWebhookLog] (
        [Id] bigint NOT NULL IDENTITY(1,1),
        [Provider] nvarchar(50) NOT NULL,
        [EventId] nvarchar(100) NULL,
        [Payload] nvarchar(max) NOT NULL,
        [SignatureValid] bit NOT NULL,
        [ProcessingStatus] nvarchar(30) NOT NULL,
        [ReceivedAt] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        [ErrorMessage] nvarchar(max) NULL,
        CONSTRAINT [PK_PaymentWebhookLog] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_PaymentWebhookLog_ReceivedAt] ON [PaymentWebhookLog] ([ReceivedAt]);
END;

IF OBJECT_ID(N'[DotThuHocPhi]', N'U') IS NULL
BEGIN
    CREATE TABLE [DotThuHocPhi] (
        [Id] int NOT NULL IDENTITY(1,1),
        [MaKhoanThu] nvarchar(50) NOT NULL,
        [TenKhoanThu] nvarchar(200) NOT NULL,
        [MoTa] nvarchar(500) NULL,
        [HocKyId] int NOT NULL,
        [SoTien] decimal(18,2) NOT NULL,
        [NgayBatDau] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        [HanThanhToan] date NULL,
        [DoiTuongApDung] nvarchar(30) NOT NULL DEFAULT 'TOAN_TRUONG',
        [GiaTriApDung] nvarchar(255) NULL,
        [TrangThai] nvarchar(20) NOT NULL DEFAULT 'ACTIVE',
        [NguoiTao] nvarchar(100) NULL,
        [NgayTao] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        CONSTRAINT [PK_DotThuHocPhi] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_DotThuHocPhi_MaKhoanThu] ON [DotThuHocPhi] ([MaKhoanThu]);
END;

IF OBJECT_ID(N'[NhatKyThanhToan]', N'U') IS NULL
BEGIN
    CREATE TABLE [NhatKyThanhToan] (
        [Id] bigint NOT NULL IDENTITY(1,1),
        [HanhDong] nvarchar(100) NOT NULL,
        [DoiTuong] nvarchar(100) NOT NULL,
        [KhoaChinh] nvarchar(100) NULL,
        [NguoiThucHien] nvarchar(100) NOT NULL,
        [ThoiGian] datetime2 NOT NULL DEFAULT sysutcdatetime(),
        [LyDo] nvarchar(500) NOT NULL,
        [BangChungDoiChieu] nvarchar(500) NULL,
        [TrangThaiTruoc] nvarchar(100) NULL,
        [TrangThaiSau] nvarchar(100) NULL,
        [ChiTiet] nvarchar(max) NULL,
        CONSTRAINT [PK_NhatKyThanhToan] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_NhatKyThanhToan_ThoiGian] ON [NhatKyThanhToan] ([ThoiGian]);
END;
";
            await db.Database.ExecuteSqlRawAsync(sql);
            logger.LogInformation("Khởi tạo an toàn cấu trúc bảng Thanh toán VietQR & Học phí thành công.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Lỗi khi chạy script EnsurePaymentTablesAsync.");
        }
    }
}
