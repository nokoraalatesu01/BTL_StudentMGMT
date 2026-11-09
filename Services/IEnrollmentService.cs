using System.Threading;
using System.Threading.Tasks;
using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Services;

public enum EnrollmentActor
{
    Student,
    Admin
}

public enum EnrollmentErrorCode
{
    None = 0,
    NotFound,
    ClassNotOpen,
    AlreadyEnrolled,
    ClassFull,
    SameSubjectEnrolled,
    ScheduleConflict,
    HasGradesCannotDrop,
    CannotDropClosedClass,
    DbError
}

public class EnrollResult
{
    public bool Success { get; set; }
    public EnrollmentErrorCode ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DangKyHocPhan? Enrollment { get; set; }
    public LopHocPhan? LopHocPhan { get; set; }

    public static EnrollResult Ok(DangKyHocPhan enrollment, LopHocPhan lopHocPhan, string message = "") =>
        new() { Success = true, Enrollment = enrollment, LopHocPhan = lopHocPhan, Message = message };

    public static EnrollResult Fail(EnrollmentErrorCode code, string message) =>
        new() { Success = false, ErrorCode = code, Message = message };
}

public class DropResult
{
    public bool Success { get; set; }
    public EnrollmentErrorCode ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public LopHocPhan? LopHocPhan { get; set; }

    public static DropResult Ok(LopHocPhan? lopHocPhan = null, string message = "") =>
        new() { Success = true, LopHocPhan = lopHocPhan, Message = message };

    public static DropResult Fail(EnrollmentErrorCode code, string message) =>
        new() { Success = false, ErrorCode = code, Message = message };
}

public interface IEnrollmentService
{
    Task<EnrollResult> EnrollAsync(int sinhVienId, int lopHocPhanId, EnrollmentActor actor, CancellationToken cancellationToken = default);
    Task<DropResult> DropAsync(int sinhVienId, int lopHocPhanId, EnrollmentActor actor, CancellationToken cancellationToken = default);
    Task AcquireClassLockAsync(int lopHocPhanId, CancellationToken cancellationToken = default);
}
