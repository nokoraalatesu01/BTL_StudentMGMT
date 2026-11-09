using System.Linq;

namespace BTL_StudentMGMT.Models;

public static class DangKyHocPhanExtensions
{
    public static IQueryable<DangKyHocPhan> Enrolled(this IQueryable<DangKyHocPhan> query)
    {
        return query.Where(dk => dk.TrangThai == EnrollmentStatus.Enrolled);
    }
}
