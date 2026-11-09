using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Helpers;
using BTL_StudentMGMT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BTL_StudentMGMT.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _db;

    public EnrollmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AcquireClassLockAsync(int lopHocPhanId, CancellationToken cancellationToken = default)
    {
        if (_db.Database.IsSqlServer())
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM dbo.LopHocPhan WITH (UPDLOCK, ROWLOCK) WHERE Id = {lopHocPhanId}",
                cancellationToken);
        }
    }

    public async Task<EnrollResult> EnrollAsync(int sinhVienId, int lopHocPhanId, EnrollmentActor actor, CancellationToken cancellationToken = default)
    {
        var student = await _db.SinhViens.FirstOrDefaultAsync(s => s.Id == sinhVienId, cancellationToken);
        if (student == null)
        {
            return EnrollResult.Fail(EnrollmentErrorCode.NotFound, "Sinh viên không tồn tại.");
        }

        var lhp = await _db.LopHocPhans
            .Include(l => l.MonHoc)
            .FirstOrDefaultAsync(l => l.Id == lopHocPhanId, cancellationToken);

        if (lhp == null)
        {
            return EnrollResult.Fail(EnrollmentErrorCode.NotFound, "Lớp học phần không tồn tại.");
        }

        // Sinh viên chỉ được đăng ký lớp OPEN; Admin được phép thêm sinh viên vào lớp dù trạng thái không phải OPEN
        if (actor == EnrollmentActor.Student && lhp.TrangThai != "OPEN")
        {
            return EnrollResult.Fail(EnrollmentErrorCode.ClassNotOpen, "Lớp học phần đã đóng đăng ký hoặc kết thúc.");
        }

        // 1. Kiểm tra đã đăng ký (chỉ tính ENROLLED)
        var alreadyEnrolled = await _db.DangKyHocPhans.AnyAsync(dk =>
            dk.SinhVienId == sinhVienId &&
            dk.LopHocPhanId == lopHocPhanId &&
            dk.TrangThai == EnrollmentStatus.Enrolled,
            cancellationToken);

        if (alreadyEnrolled)
        {
            return EnrollResult.Fail(EnrollmentErrorCode.AlreadyEnrolled,
                actor == EnrollmentActor.Admin
                    ? "Sinh viên này đã có trong danh sách đăng ký của lớp học phần."
                    : "Bạn đã đăng ký lớp học phần này rồi.");
        }

        // 2. Kiểm tra đã đăng ký nhóm khác của cùng môn học trong cùng học kỳ chưa
        var daDangKyCungMon = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
            .AnyAsync(dk =>
                dk.SinhVienId == sinhVienId &&
                dk.LopHocPhan.MonHocId == lhp.MonHocId &&
                dk.LopHocPhan.HocKyId == lhp.HocKyId &&
                dk.LopHocPhanId != lopHocPhanId &&
                dk.TrangThai == EnrollmentStatus.Enrolled,
                cancellationToken);

        if (daDangKyCungMon)
        {
            return EnrollResult.Fail(EnrollmentErrorCode.SameSubjectEnrolled,
                $"Bạn đã đăng ký một nhóm khác của môn '{lhp.MonHoc?.TenMon}' trong học kỳ này.");
        }

        // 3. Kiểm tra trùng lịch học: chỉ so với đăng ký ENROLLED cùng HocKyId
        var currentEnrollments = await _db.DangKyHocPhans
            .Include(dk => dk.LopHocPhan)
            .Where(dk => dk.SinhVienId == sinhVienId &&
                         dk.TrangThai == EnrollmentStatus.Enrolled &&
                         dk.LopHocPhan.HocKyId == lhp.HocKyId &&
                         dk.LopHocPhanId != lopHocPhanId)
            .Select(dk => dk.LopHocPhan)
            .ToListAsync(cancellationToken);

        var trungLich = currentEnrollments.FirstOrDefault(other => ScheduleConflict.Overlaps(lhp, other));
        if (trungLich != null)
        {
            var caTen = CaHocHelper.GetTenCa(CaHocHelper.GetCaTuTiet(lhp.TietBatDau));
            var khungGio = CaHocHelper.GetKhungGio(lhp.TietBatDau, lhp.SoTiet);
            return EnrollResult.Fail(EnrollmentErrorCode.ScheduleConflict,
                $"Trùng lịch học: Lớp này học vào {lhp.Thu}, {caTen} ({khungGio}), trùng với môn '{trungLich.TenLopHocPhan}'.");
        }

        // 4. Mở Transaction và Khóa dòng LopHocPhan (L9)
        IDbContextTransaction? tx = null;
        try
        {
            if (_db.Database.CurrentTransaction == null && !IsInMemoryDatabase())
            {
                tx = await _db.Database.BeginTransactionAsync(cancellationToken);
            }
        }
        catch
        {
            // Provider không hỗ trợ transaction (ví dụ InMemory)
        }

        try
        {
            await AcquireClassLockAsync(lopHocPhanId, cancellationToken);

            // Đếm lại số lượng ENROLLED bên trong khóa dòng
            var countEnrolled = await _db.DangKyHocPhans
                .CountAsync(dk => dk.LopHocPhanId == lopHocPhanId && dk.TrangThai == EnrollmentStatus.Enrolled, cancellationToken);

            if (countEnrolled >= lhp.SiSoToiDa)
            {
                if (tx != null) await tx.RollbackAsync(cancellationToken);
                return EnrollResult.Fail(EnrollmentErrorCode.ClassFull,
                    actor == EnrollmentActor.Admin
                        ? "Lớp học phần đã đạt sĩ số tối đa."
                        : "Lớp học phần đã đầy sĩ số (hết chỗ). Vui lòng chọn nhóm lớp khác.");
            }

            // Kiểm tra lại trạng thái đã đăng ký trong transaction để tránh race condition
            var alreadyEnrolledInTx = await _db.DangKyHocPhans.AnyAsync(dk =>
                dk.SinhVienId == sinhVienId &&
                dk.LopHocPhanId == lopHocPhanId &&
                dk.TrangThai == EnrollmentStatus.Enrolled,
                cancellationToken);

            if (alreadyEnrolledInTx)
            {
                if (tx != null) await tx.RollbackAsync(cancellationToken);
                return EnrollResult.Fail(EnrollmentErrorCode.AlreadyEnrolled,
                    actor == EnrollmentActor.Admin
                        ? "Sinh viên này đã có trong danh sách đăng ký của lớp học phần."
                        : "Bạn đã đăng ký lớp học phần này rồi.");
            }

            // 5. Phân tích lịch sử học môn này để tự động xác định loại đăng ký và số lần học
            var previousAttempts = await _db.Diems
                .Where(d => d.SinhVienId == sinhVienId && d.MonHocId == lhp.MonHocId)
                .OrderByDescending(d => d.LanHoc)
                .ToListAsync(cancellationToken);

            string loaiDangKy = "HOC_LAN_DAU";
            int lanHoc = 1;
            string? ghiChuDk = null;

            if (previousAttempts.Count > 0)
            {
                var lastAttempt = previousAttempts.First();
                lanHoc = previousAttempts.Max(d => d.LanHoc) + 1;
                if (lastAttempt.DiemTongKet < 4.0)
                {
                    loaiDangKy = "HOC_LAI";
                    ghiChuDk = $"Đăng ký học lại do lần {lastAttempt.LanHoc} không đạt ({lastAttempt.DiemTongKet} điểm)";
                }
                else
                {
                    loaiDangKy = "HOC_CAI_THIEN";
                    ghiChuDk = $"Đăng ký học cải thiện điểm (lần {lastAttempt.LanHoc} đã đạt {lastAttempt.DiemTongKet} điểm)";
                }
            }
            else
            {
                var prevDk = await _db.DangKyHocPhans
                    .Include(dk => dk.LopHocPhan)
                    .Where(dk => dk.SinhVienId == sinhVienId &&
                                 dk.LopHocPhan.MonHocId == lhp.MonHocId &&
                                 dk.LopHocPhanId != lopHocPhanId &&
                                 dk.TrangThai == EnrollmentStatus.Enrolled)
                    .CountAsync(cancellationToken);
                if (prevDk > 0)
                {
                    lanHoc = prevDk + 1;
                    loaiDangKy = "HOC_LAI";
                }
            }

            // 6. Đăng ký lại sau khi đã hủy: TÁI SỬ DỤNG bản ghi DROPPED nếu có
            var existingRecord = await _db.DangKyHocPhans.FirstOrDefaultAsync(dk =>
                dk.SinhVienId == sinhVienId && dk.LopHocPhanId == lopHocPhanId,
                cancellationToken);

            if (existingRecord != null)
            {
                existingRecord.TrangThai = EnrollmentStatus.Enrolled;
                existingRecord.LoaiDangKy = loaiDangKy;
                existingRecord.LanHoc = lanHoc;
                existingRecord.GhiChu = ghiChuDk;
                existingRecord.NgayDangKy = DateTime.UtcNow;
            }
            else
            {
                existingRecord = new DangKyHocPhan
                {
                    SinhVienId = sinhVienId,
                    LopHocPhanId = lopHocPhanId,
                    LoaiDangKy = loaiDangKy,
                    LanHoc = lanHoc,
                    GhiChu = ghiChuDk,
                    NgayDangKy = DateTime.UtcNow,
                    TrangThai = EnrollmentStatus.Enrolled
                };
                _db.DangKyHocPhans.Add(existingRecord);
            }

            await _db.SaveChangesAsync(cancellationToken);
            if (tx != null) await tx.CommitAsync(cancellationToken);

            var successMsg = actor == EnrollmentActor.Admin
                ? "Đã thêm sinh viên vào lớp học phần thành công."
                : $"Đăng ký thành công lớp học phần '{lhp.TenLopHocPhan}'!";

            return EnrollResult.Ok(existingRecord, lhp, successMsg);
        }
        catch (DbUpdateException)
        {
            if (tx != null) await tx.RollbackAsync(cancellationToken);
            // Bắt DbUpdateException do UQ_DK_SV_LHP (bấm đúp)
            return EnrollResult.Fail(EnrollmentErrorCode.AlreadyEnrolled,
                actor == EnrollmentActor.Admin
                    ? "Sinh viên này đã có trong danh sách đăng ký của lớp học phần."
                    : "Bạn đã đăng ký lớp học phần này rồi.");
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (tx != null) await tx.DisposeAsync();
        }
    }

    public async Task<DropResult> DropAsync(int sinhVienId, int lopHocPhanId, EnrollmentActor actor, CancellationToken cancellationToken = default)
    {
        var dk = await _db.DangKyHocPhans
            .Include(d => d.LopHocPhan)
            .FirstOrDefaultAsync(d => d.SinhVienId == sinhVienId &&
                                      d.LopHocPhanId == lopHocPhanId &&
                                      d.TrangThai == EnrollmentStatus.Enrolled,
                                 cancellationToken);

        if (dk == null)
        {
            return DropResult.Fail(EnrollmentErrorCode.NotFound,
                actor == EnrollmentActor.Admin
                    ? "Không tìm thấy bản ghi đăng ký của sinh viên trong lớp học phần này."
                    : "Bản ghi đăng ký không tồn tại.");
        }

        // Sinh viên chỉ được hủy khi lớp còn OPEN; Admin được phép hủy theo quy chế quản trị
        if (actor == EnrollmentActor.Student && dk.LopHocPhan.TrangThai != "OPEN")
        {
            return DropResult.Fail(EnrollmentErrorCode.CannotDropClosedClass,
                "Lớp học phần đã khóa đăng ký, không thể hủy.");
        }

        // Kiểm tra điểm của cặp (sinh viên, lớp)
        var diems = await _db.Diems
            .Where(d => d.SinhVienId == sinhVienId && d.LopHocPhanId == lopHocPhanId)
            .ToListAsync(cancellationToken);

        var diemThanhPhans = await _db.DiemThanhPhans
            .Where(dtp => dtp.DangKyId == dk.Id)
            .ToListAsync(cancellationToken);

        // Có bất kỳ điểm thành phần nào khác null hoặc điểm tổng kết > 0
        bool hasEnteredGrades = diems.Any(d =>
            d.DiemChuyenCan > 0 ||
            d.DiemGiuaKy > 0 ||
            d.DiemCuoiKy > 0 ||
            d.DiemCuoiKyLan2.HasValue ||
            d.DiemTongKet > 0);

        bool hasEnteredDiemThanhPhan = diemThanhPhans.Any(dtp => dtp.Diem.HasValue);

        if (hasEnteredGrades || hasEnteredDiemThanhPhan)
        {
            return DropResult.Fail(EnrollmentErrorCode.HasGradesCannotDrop,
                actor == EnrollmentActor.Admin
                    ? "Không thể hủy sinh viên đã có điểm tổng kết trong lớp học phần này."
                    : "Không thể hủy đăng ký lớp học phần đã có điểm đánh giá.");
        }

        // Mở transaction dọn dẹp và xóa mềm
        IDbContextTransaction? tx = null;
        try
        {
            if (_db.Database.CurrentTransaction == null && !IsInMemoryDatabase())
            {
                tx = await _db.Database.BeginTransactionAsync(cancellationToken);
            }
        }
        catch
        {
            // Provider không hỗ trợ transaction
        }

        try
        {
            // Nếu có bản ghi Diem rỗng thì xóa để tránh mồ côi
            if (diems.Count > 0)
            {
                _db.Diems.RemoveRange(diems);
            }

            if (diemThanhPhans.Count > 0)
            {
                _db.DiemThanhPhans.RemoveRange(diemThanhPhans);
            }

            // Dọn điểm danh của sinh viên trong lớp học phần này
            var diemDanhs = await _db.DiemDanhs
                .Where(dd => dd.SinhVienId == sinhVienId && dd.LopHocPhanId == lopHocPhanId)
                .ToListAsync(cancellationToken);

            if (diemDanhs.Count > 0)
            {
                _db.DiemDanhs.RemoveRange(diemDanhs);
            }

            // Hủy đăng ký chuyển sang xóa mềm: TrangThai = "DROPPED"
            dk.TrangThai = EnrollmentStatus.Dropped;

            await _db.SaveChangesAsync(cancellationToken);
            if (tx != null) await tx.CommitAsync(cancellationToken);

            var msg = actor == EnrollmentActor.Admin
                ? "Đã xóa sinh viên khỏi lớp học phần."
                : $"Đã hủy đăng ký lớp học phần '{dk.LopHocPhan.TenLopHocPhan}'.";

            return DropResult.Ok(dk.LopHocPhan, msg);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (tx != null) await tx.DisposeAsync();
        }
    }

    private bool IsInMemoryDatabase()
    {
        var provider = _db.Database.ProviderName;
        return provider != null && provider.EndsWith("InMemory", StringComparison.OrdinalIgnoreCase);
    }
}
