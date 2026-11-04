using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Services;

public interface IPaymentService
{
    // Phân hệ Sinh viên
    Task<SinhVienHocPhiViewModel> GetSinhVienHocPhiAsync(int sinhVienId, int? hocKyId, string? trangThai);
    Task<TaoPaymentResponse> CreatePaymentAsync(long hoaDonId, int sinhVienId);
    Task<CheckPaymentStatusResponse> CheckPaymentStatusAsync(string transactionCode, int? sinhVienId = null);

    // Xử lý tự động VietQR Webhook & Sandbox
    Task<(bool Success, string Message)> ProcessWebhookAsync(VietQrWebhookPayload payload, string rawPayload, string? signatureHeader);
    Task<(bool Success, string Message)> SimulateTransferAsync(string transactionCode, decimal? customAmount = null);

    // Phân hệ Quản trị Admin
    Task<AdminPaymentDashboardViewModel> GetAdminDashboardAsync(int? namHocId, int? hocKyId, int? thang, string? tuNgay, string? denNgay, int? lopId, int? nganhId);
    
    // Quản lý khoản thu
    Task<AdminKhoanThuListViewModel> GetKhoanThuListAsync(int? hocKyId, string? trangThai, string? search);
    Task<AdminTaoKhoanThuViewModel> GetTaoKhoanThuModelAsync();
    Task<(bool Success, string Message, int Id)> TaoKhoanThuAsync(AdminTaoKhoanThuViewModel model, string nguoiTao);
    Task<AdminChiTietKhoanThuViewModel?> GetChiTietKhoanThuAsync(int id);
    Task<(bool Success, string Message)> NgungKhoanThuAsync(int id, string nguoiThucHien);
    Task<(bool Success, string Message)> CapNhatKhoanThuAsync(int id, string tenKhoanThu, string? moTa, DateOnly? hanThanhToan, decimal? soTienMoi, string nguoiThucHien);

    // Quản lý giao dịch
    Task<AdminGiaoDichListViewModel> GetGiaoDichListAsync(int page, int pageSize, string? search, string? status, string? bankId, string? tuNgay, string? denNgay);
    Task<AdminChiTietGiaoDichViewModel?> GetChiTietGiaoDichAsync(long id);

    // Quản lý công nợ
    Task<AdminCongNoListViewModel> GetCongNoListAsync(int page, int pageSize, string? search, int? lopId, int? nganhId, int? hocKyId, bool chiSinhVienNo);
    Task<AdminChiTietCongNoViewModel?> GetChiTietCongNoAsync(int sinhVienId);
    Task<(bool Success, string Message)> DieuChinhCongNoAsync(DieuChinhCongNoRequest request, string nguoiThucHien);

    // Đối soát ngân hàng
    Task<AdminDoiSoatViewModel> GetDoiSoatListAsync(string? ketQuaFilter, string? tuNgay, string? denNgay, string? search);
    Task<(bool Success, string Message)> XuLyBatThuongDoiSoatAsync(XuLyBatThuongRequest request, string nguoiThucHien);

    // Audit Log
    Task GhiNhatKyAuditAsync(string hanhDong, string doiTuong, string? khoaChinh, string nguoiThucHien, string lyDo, string? bangChung = null, string? trangThaiTruoc = null, string? trangThaiSau = null, string? chiTiet = null);
}
