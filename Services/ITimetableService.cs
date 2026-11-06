using System;
using System.Threading.Tasks;
using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Services;

public interface ITimetableService
{
    Task<GiangVienLichGiangDayViewModel> GetGiangVienLichGiangDayAsync(
        int giangVienId,
        int? hocKyId = null,
        int? namHocId = null,
        int? monHocId = null,
        string? ngay = null,
        string? viewMode = "week");

    Task<SinhVienThoiKhoaBieuViewModel> GetSinhVienThoiKhoaBieuAsync(
        int sinhVienId,
        int? hocKyId = null,
        int? namHocId = null,
        int? monHocId = null,
        string? ngay = null,
        string? viewMode = "week");

    Task<ThoiKhoaBieuCardViewModel?> GetChiTietBuoiHocAsync(
        int lopHocPhanId,
        DateTime? ngay = null,
        int? currentSinhVienId = null,
        int? currentGiangVienId = null);
}
