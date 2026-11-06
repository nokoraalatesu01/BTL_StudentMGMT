using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BTL_StudentMGMT.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly IVietQrService _vietQrService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext db,
        IVietQrService vietQrService,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _vietQrService = vietQrService;
        _logger = logger;
    }

    // =========================================================================
    // I. PHÂN HỆ SINH VIÊN
    // =========================================================================

    public async Task<SinhVienHocPhiViewModel> GetSinhVienHocPhiAsync(int sinhVienId, int? hocKyId, string? trangThai)
    {
        var student = await _db.SinhViens
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .FirstOrDefaultAsync(s => s.Id == sinhVienId);

        if (student == null)
        {
            throw new ArgumentException("Không tìm thấy hồ sơ sinh viên.");
        }

        // Tự động kiểm tra và tạo khoản thu mẫu nếu sinh viên chưa có hóa đơn học phí
        await EnsureHocPhiInitializedAsync(student);

        var query = _db.HoaDonHocPhis
            .Include(h => h.HocKy)
                .ThenInclude(hk => hk!.NamHoc)
            .Where(h => h.SinhVienId == sinhVienId);

        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            query = query.Where(h => h.HocKyId == hocKyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            if (trangThai == "DANG_XU_LY")
            {
                var activePendingHdIds = await _db.Payments
                    .Where(p => p.SinhVienId == sinhVienId && p.Status == "PENDING" && p.ExpiredAt > DateTime.UtcNow)
                    .Select(p => p.HoaDonHocPhiId)
                    .Distinct()
                    .ToListAsync();
                query = query.Where(h => activePendingHdIds.Contains(h.Id));
            }
            else
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }
        }

        var list = await query.OrderByDescending(h => h.NgayLap).ToListAsync();

        var allHocKies = await _db.HocKies
            .Include(h => h.NamHoc)
            .OrderByDescending(h => h.NgayBatDau)
            .ToListAsync();

        var config = await _vietQrService.GetConfigAsync();

        var allStudentInvoices = await _db.HoaDonHocPhis
            .Where(h => h.SinhVienId == sinhVienId)
            .ToListAsync();

        var tongHocPhi = allStudentInvoices.Sum(h => h.SoTien);
        var daThanhToan = allStudentInvoices.Sum(h => h.DaThanhToan);
        var soKhoanConNo = allStudentInvoices.Count(h => h.ConPhaiDong > 0);

        var pendingPayments = await _db.Payments
            .Where(p => p.SinhVienId == sinhVienId && p.Status == "PENDING" && p.ExpiredAt > DateTime.UtcNow)
            .ToListAsync();

        return new SinhVienHocPhiViewModel
        {
            SinhVien = student,
            TongHocPhi = tongHocPhi,
            DaThanhToan = daThanhToan,
            SoKhoanConNo = soKhoanConNo,
            DanhSachHocKy = allHocKies,
            SelectedHocKyId = hocKyId,
            SelectedTrangThai = trangThai,
            VietQrConfig = config,
            DanhSachHocPhi = list.Select(h =>
            {
                var pending = pendingPayments.FirstOrDefault(p => p.HoaDonHocPhiId == h.Id);
                return new HoaDonHocPhiItemViewModel
                {
                    Id = h.Id,
                    SoHoaDon = h.SoHoaDon,
                    TenKhoanThu = h.TenKhoanThu,
                    TenHocKy = h.HocKy?.TenHocKy ?? "Học kỳ chính",
                    TenNamHoc = h.HocKy?.NamHoc?.MaNamHoc,
                    SoTien = h.SoTien,
                    DaThanhToan = h.DaThanhToan,
                    NgayLap = h.NgayLap,
                    HanThanhToan = h.HanThanhToan,
                    TrangThai = h.TrangThai,
                    GhiChu = h.GhiChu,
                    DangXuLy = pending != null,
                    ActiveTransactionCode = pending?.TransactionCode
                };
            }).ToList()
        };
    }

    public async Task<TaoPaymentResponse> CreatePaymentAsync(long hoaDonId, int sinhVienId)
    {
        var hoaDon = await _db.HoaDonHocPhis
            .Include(h => h.SinhVien)
            .Include(h => h.HocKy)
            .FirstOrDefaultAsync(h => h.Id == hoaDonId && h.SinhVienId == sinhVienId);

        if (hoaDon == null)
        {
            return new TaoPaymentResponse { Success = false, Message = "Không tìm thấy khoản thu học phí hợp lệ." };
        }

        if (hoaDon.ConPhaiDong <= 0)
        {
            return new TaoPaymentResponse { Success = false, Message = "Khoản học phí này đã được thanh toán đầy đủ." };
        }

        var config = await _vietQrService.GetConfigAsync();
        var amount = hoaDon.ConPhaiDong;

        // Chống tạo thanh toán trùng lặp: Nếu đã có một giao dịch PENDING còn hiệu lực cho hóa đơn này, tái sử dụng nó
        var existingPendingPayment = await _db.Payments
            .Where(p => p.HoaDonHocPhiId == hoaDon.Id 
                     && p.SinhVienId == sinhVienId 
                     && p.Status == "PENDING" 
                     && p.ExpiredAt > DateTime.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (existingPendingPayment != null)
        {
            return new TaoPaymentResponse
            {
                Success = true,
                TransactionCode = existingPendingPayment.TransactionCode,
                QrUrl = existingPendingPayment.QrUrl ?? "",
                BankId = config.BankId,
                BankName = config.BankName,
                AccountNumber = config.AccountNumber,
                AccountHolderName = config.AccountHolderName,
                Amount = existingPendingPayment.Amount,
                PaymentContent = existingPendingPayment.PaymentContent,
                ExpiredAt = existingPendingPayment.ExpiredAt,
                IsSandbox = string.Equals(config.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase)
            };
        }

        // Sinh mã giao dịch duy nhất
        var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        var datePrefix = DateTime.UtcNow.ToString("yyMMdd");
        var transactionCode = $"HP{datePrefix}{uniqueSuffix}";

        // Chuẩn hóa cú pháp nội dung chuyển khoản VietQR: HP <MaSV> <Code>
        var rawMsv = hoaDon.SinhVien?.MaSinhVien ?? "SV";
        var cleanMsv = Regex.Replace(rawMsv, @"\s+", "");
        var paymentContent = $"HP {cleanMsv} {transactionCode}";

        // Tạo ảnh VietQR động
        var qrUrl = _vietQrService.GenerateVietQrUrl(amount, paymentContent, config);

        var payment = new Payment
        {
            TransactionCode = transactionCode,
            HoaDonHocPhiId = hoaDon.Id,
            SinhVienId = sinhVienId,
            Amount = amount,
            PaymentContent = paymentContent,
            PaymentMethod = "VIETQR_VCB",
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddMinutes(20),
            QrUrl = qrUrl,
            IsAutoVerified = false
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return new TaoPaymentResponse
        {
            Success = true,
            TransactionCode = transactionCode,
            QrUrl = qrUrl,
            BankId = config.BankId,
            BankName = config.BankName,
            AccountNumber = config.AccountNumber,
            AccountHolderName = config.AccountHolderName,
            Amount = amount,
            PaymentContent = paymentContent,
            ExpiredAt = payment.ExpiredAt,
            IsSandbox = string.Equals(config.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase)
        };
    }

    public async Task<CheckPaymentStatusResponse> CheckPaymentStatusAsync(string transactionCode, int? sinhVienId = null)
    {
        var query = _db.Payments.Where(p => p.TransactionCode == transactionCode);
        if (sinhVienId.HasValue)
        {
            query = query.Where(p => p.SinhVienId == sinhVienId.Value);
        }

        var payment = await query.FirstOrDefaultAsync();
        if (payment == null)
        {
            return new CheckPaymentStatusResponse
            {
                Success = false,
                Status = "NOT_FOUND",
                Message = "Không tìm thấy giao dịch."
            };
        }

        // Tự động hết hạn nếu đã quá ExpiredAt và chưa Paid
        if (payment.Status == "PENDING" && payment.ExpiredAt < DateTime.UtcNow)
        {
            payment.Status = "EXPIRED";
            await _db.SaveChangesAsync();
        }

        return new CheckPaymentStatusResponse
        {
            Success = true,
            Status = payment.Status,
            Amount = payment.Amount,
            PaidAt = payment.PaidAt,
            TransactionCode = payment.TransactionCode,
            BankTransactionId = payment.BankTransactionId,
            Message = payment.Status == "PAID" ? "Giao dịch đã được xác nhận thanh toán thành công!" : "Đang chờ thanh toán..."
        };
    }

    // =========================================================================
    // II. TỰ ĐỘNG XỬ LÝ VIETQR WEBHOOK & SANDBOX
    // =========================================================================

    public async Task<(bool Success, string Message)> ProcessWebhookAsync(VietQrWebhookPayload payload, string rawPayload, string? signatureHeader)
    {
        var config = await _vietQrService.GetConfigAsync();
        var isSandbox = string.Equals(config.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase);

        // 1. Kiểm tra chữ ký (Signature)
        bool signatureValid = true;
        if (!isSandbox)
        {
            var headerSig = signatureHeader ?? payload.Signature;
            signatureValid = _vietQrService.VerifyWebhookSignature(rawPayload, headerSig, config.WebhookSecret);
        }

        // Lưu log webhook ngay lập tức
        var log = new PaymentWebhookLog
        {
            Provider = "VIETQR_VCB",
            EventId = payload.TransactionId ?? payload.BankTransactionId,
            Payload = rawPayload,
            SignatureValid = signatureValid,
            ProcessingStatus = signatureValid ? "RECEIVED" : "INVALID_SIGNATURE",
            ReceivedAt = DateTime.UtcNow
        };
        _db.PaymentWebhookLogs.Add(log);
        await _db.SaveChangesAsync();

        if (!signatureValid)
        {
            log.ErrorMessage = "Chữ ký số HMAC-SHA256 của Webhook không khớp.";
            log.ProcessingStatus = "REJECTED";
            await _db.SaveChangesAsync();
            return (false, "Invalid Webhook Signature");
        }

        // 2. Trích xuất mã giao dịch nội bộ từ nội dung chuyển khoản
        var content = payload.Content ?? "";
        var match = Regex.Match(content, @"HP\s+[A-Za-z0-9]+\s+(HP\d{6}[A-Za-z0-9]+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            match = Regex.Match(content, @"(HP\d{6}[A-Za-z0-9]{6})", RegexOptions.IgnoreCase);
        }

        if (!match.Success)
        {
            log.ErrorMessage = $"Không tìm thấy cú pháp mã giao dịch hợp lệ trong nội dung: '{content}'";
            log.ProcessingStatus = "CONTENT_NOT_RECOGNIZED";
            await _db.SaveChangesAsync();
            return (false, "Transaction code not found in transfer content");
        }

        var transactionCode = match.Groups[match.Groups.Count - 1].Value.ToUpperInvariant();

        // 3. Xử lý giao dịch an toàn trong DB Transaction
        using var dbTransaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var payment = await _db.Payments
                .Include(p => p.HoaDonHocPhi)
                .FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);

            if (payment == null)
            {
                log.ErrorMessage = $"Không tìm thấy Payment nội bộ với mã: {transactionCode}";
                log.ProcessingStatus = "PAYMENT_NOT_FOUND";
                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (false, "Payment order not found");
            }

            // Chống xử lý trùng lặp (Idempotency)
            if (payment.Status == "PAID")
            {
                log.ProcessingStatus = "ALREADY_PROCESSED";
                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (true, "Payment already verified previously.");
            }

            // Kiểm tra số tiền chuyển
            if (payload.Amount < payment.Amount)
            {
                payment.Status = "AMOUNT_MISMATCH";
                log.ErrorMessage = $"Số tiền chuyển ({payload.Amount:N0}đ) nhỏ hơn số tiền cần thanh toán ({payment.Amount:N0}đ).";
                log.ProcessingStatus = "AMOUNT_MISMATCH";
                await _db.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return (false, "Transfer amount is insufficient");
            }

            var bankTxnId = payload.BankTransactionId ?? payload.TransactionId ?? $"VCB-{DateTime.UtcNow:yyyyMMddHHmmss}-{payment.Id}";

            // Chống trùng mã giao dịch ngân hàng
            var existingBankTxn = await _db.BankTransactions.AnyAsync(b => b.BankTransactionId == bankTxnId);
            if (!existingBankTxn)
            {
                var bankTxn = new BankTransaction
                {
                    BankTransactionId = bankTxnId,
                    TransactionId = payload.TransactionId,
                    Amount = payload.Amount,
                    TransactionContent = content,
                    TransactionDate = payload.TransactionDate ?? DateTime.UtcNow,
                    Direction = "IN",
                    PaymentId = payment.Id,
                    ProcessedAt = DateTime.UtcNow,
                    RawData = rawPayload
                };
                _db.BankTransactions.Add(bankTxn);
            }

            // Cập nhật trạng thái Payment
            payment.Status = "PAID";
            payment.PaidAt = DateTime.UtcNow;
            payment.BankTransactionId = bankTxnId;
            payment.IsAutoVerified = true;
            payment.RawWebhookData = rawPayload;

            // Cập nhật công nợ Hóa đơn học phí
            if (payment.HoaDonHocPhi != null)
            {
                payment.HoaDonHocPhi.DaThanhToan += payment.Amount;
                if (payment.HoaDonHocPhi.ConPhaiDong <= 0)
                {
                    payment.HoaDonHocPhi.TrangThai = "DA_THANH_TOAN";
                }
                else
                {
                    payment.HoaDonHocPhi.TrangThai = "THANH_TOAN_MOT_PHAN";
                }
            }

            log.ProcessingStatus = "SUCCESS";
            await _db.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            _logger.LogInformation("Thanh toán tự động thành công cho mã GD {Code}, số tiền {Amount:N0}đ", transactionCode, payment.Amount);
            return (true, "Payment verified successfully");
        }
        catch (Exception ex)
        {
            await dbTransaction.RollbackAsync();
            _logger.LogError(ex, "Lỗi trong quá trình xử lý Webhook cho mã {Code}", transactionCode);
            log.ErrorMessage = ex.Message;
            log.ProcessingStatus = "EXCEPTION";
            await _db.SaveChangesAsync();
            return (false, "Internal server error processing webhook");
        }
    }

    public async Task<(bool Success, string Message)> SimulateTransferAsync(string transactionCode, decimal? customAmount = null)
    {
        var config = await _vietQrService.GetConfigAsync();
        if (!string.Equals(config.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Chức năng mô phỏng thanh toán chỉ được phép hoạt động trên môi trường Sandbox (thử nghiệm)!");
        }

        var payment = await _db.Payments
            .Include(p => p.SinhVien)
            .FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);

        if (payment == null)
        {
            return (false, "Không tìm thấy giao dịch cần mô phỏng.");
        }

        var amount = customAmount ?? payment.Amount;
        var fakeBankTxnId = $"VCB-SIM-{DateTime.UtcNow:yyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant()}";
        var content = payment.PaymentContent;

        var payload = new VietQrWebhookPayload
        {
            TransactionId = fakeBankTxnId,
            BankTransactionId = fakeBankTxnId,
            Amount = amount,
            Content = content,
            TransactionDate = DateTime.UtcNow,
            AccountNumber = "1043572004",
            Gateway = "Vietcombank_Sandbox_Simulator"
        };

        var rawJson = System.Text.Json.JsonSerializer.Serialize(payload);
        return await ProcessWebhookAsync(payload, rawJson, "SIMULATOR_BYPASS");
    }

    // =========================================================================
    // III. PHÂN HỆ QUẢN TRỊ ADMIN - DASHBOARD TỔNG QUAN
    // =========================================================================

    public async Task<AdminPaymentDashboardViewModel> GetAdminDashboardAsync(
        int? namHocId, int? hocKyId, int? thang, string? tuNgay, string? denNgay, int? lopId, int? nganhId)
    {
        // 1. Query Hóa đơn học phí theo bộ lọc
        var hoaDonQuery = _db.HoaDonHocPhis
            .Include(h => h.SinhVien)
                .ThenInclude(s => s.Lop)
                    .ThenInclude(l => l!.Nganh)
            .Include(h => h.HocKy)
                .ThenInclude(hk => hk!.NamHoc)
            .AsQueryable();

        if (namHocId.HasValue && namHocId.Value > 0)
        {
            hoaDonQuery = hoaDonQuery.Where(h => h.HocKy.NamHocId == namHocId.Value);
        }
        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            hoaDonQuery = hoaDonQuery.Where(h => h.HocKyId == hocKyId.Value);
        }
        if (lopId.HasValue && lopId.Value > 0)
        {
            hoaDonQuery = hoaDonQuery.Where(h => h.SinhVien.LopId == lopId.Value);
        }
        if (nganhId.HasValue && nganhId.Value > 0)
        {
            hoaDonQuery = hoaDonQuery.Where(h => h.SinhVien.Lop != null && h.SinhVien.Lop.NganhId == nganhId.Value);
        }
        if (!string.IsNullOrWhiteSpace(tuNgay) && DateTime.TryParse(tuNgay, out var dTu))
        {
            hoaDonQuery = hoaDonQuery.Where(h => h.NgayLap >= dTu);
        }
        if (!string.IsNullOrWhiteSpace(denNgay) && DateTime.TryParse(denNgay, out var dDen))
        {
            var dDenEnd = dDen.Date.AddDays(1).AddTicks(-1);
            hoaDonQuery = hoaDonQuery.Where(h => h.NgayLap <= dDenEnd);
        }

        var hoaDons = await hoaDonQuery.ToListAsync();

        var tongHocPhiPhaiThu = hoaDons.Sum(h => h.SoTien);
        var tongTienDaThu = hoaDons.Sum(h => h.DaThanhToan);

        // 2. Query Payments theo trạng thái & số lượng
        var paymentQuery = _db.Payments.AsQueryable();
        if (thang.HasValue && thang.Value > 0)
        {
            paymentQuery = paymentQuery.Where(p => p.CreatedAt.Month == thang.Value);
        }

        var soGiaoDichThanhCong = await paymentQuery.CountAsync(p => p.Status == "PAID");
        var soGiaoDichDangCho = await paymentQuery.CountAsync(p => p.Status == "PENDING");
        var soGiaoDichThatBai = await paymentQuery.CountAsync(p => p.Status == "FAILED" || p.Status == "EXPIRED" || p.Status == "CANCELLED" || p.Status == "AMOUNT_MISMATCH");

        // 3. Biểu đồ 1: Doanh thu theo tháng (6 tháng gần nhất)
        var chartThangLabels = new List<string>();
        var chartThangData = new List<decimal>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = DateTime.UtcNow.AddMonths(-i);
            var label = $"Tháng {targetMonth.Month:D2}/{targetMonth.Year}";
            chartThangLabels.Add(label);

            var sumMonth = await _db.Payments
                .Where(p => p.Status == "PAID" && p.CreatedAt.Month == targetMonth.Month && p.CreatedAt.Year == targetMonth.Year)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
            chartThangData.Add(sumMonth);
        }

        // 4. Biểu đồ 2: Doanh thu theo học kỳ
        var hocKies = await _db.HocKies.OrderBy(h => h.NgayBatDau).Take(5).ToListAsync();
        var chartHocKyLabels = new List<string>();
        var chartHocKyData = new List<decimal>();
        foreach (var hk in hocKies)
        {
            chartHocKyLabels.Add(hk.TenHocKy);
            var sumHk = await _db.HoaDonHocPhis
                .Where(h => h.HocKyId == hk.HocKyId)
                .SumAsync(h => (decimal?)h.DaThanhToan) ?? 0m;
            chartHocKyData.Add(sumHk);
        }

        // 5. Biểu đồ 3: Tỷ lệ hoàn thành theo lớp (Top 6 lớp)
        var lops = await _db.Lops.Take(6).ToListAsync();
        var chartLopLabels = new List<string>();
        var chartLopTyLeData = new List<double>();
        foreach (var l in lops)
        {
            chartLopLabels.Add(l.TenLop);
            var hdLop = await _db.HoaDonHocPhis
                .Where(h => h.SinhVien.LopId == l.Id)
                .ToListAsync();

            if (hdLop.Count > 0 && hdLop.Sum(h => h.SoTien) > 0)
            {
                var pct = (double)(hdLop.Sum(h => h.DaThanhToan) / hdLop.Sum(h => h.SoTien) * 100);
                chartLopTyLeData.Add(Math.Round(pct, 1));
            }
            else
            {
                chartLopTyLeData.Add(0);
            }
        }

        // 6. Danh sách 10 giao dịch gần nhất & 10 logs gần nhất
        var recentPayments = await _db.Payments
            .Include(p => p.SinhVien)
                .ThenInclude(s => s.Lop)
            .Include(p => p.HoaDonHocPhi)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        var recentLogs = await _db.PaymentWebhookLogs
            .OrderByDescending(l => l.ReceivedAt)
            .Take(10)
            .ToListAsync();

        return new AdminPaymentDashboardViewModel
        {
            TongHocPhiPhaiThu = tongHocPhiPhaiThu,
            TongTienDaThu = tongTienDaThu,
            SoGiaoDichThanhCong = soGiaoDichThanhCong,
            SoGiaoDichDangCho = soGiaoDichDangCho,
            SoGiaoDichThatBai = soGiaoDichThatBai,

            SelectedNamHocId = namHocId,
            SelectedHocKyId = hocKyId,
            SelectedThang = thang,
            SelectedTuNgay = tuNgay,
            SelectedDenNgay = denNgay,
            SelectedLopId = lopId,
            SelectedNganhId = nganhId,

            DanhSachNamHoc = await _db.NamHocs.OrderByDescending(n => n.NgayBatDau).ToListAsync(),
            DanhSachHocKy = await _db.HocKies.OrderByDescending(h => h.NgayBatDau).ToListAsync(),
            DanhSachLop = await _db.Lops.OrderBy(l => l.TenLop).ToListAsync(),
            DanhSachNganh = await _db.Nganhs.OrderBy(n => n.TenNganh).ToListAsync(),

            ChartThangLabels = chartThangLabels,
            ChartThangData = chartThangData,
            ChartTyLePaid = soGiaoDichThanhCong,
            ChartTyLePending = soGiaoDichDangCho,
            ChartTyLeFailed = soGiaoDichThatBai,
            ChartHocKyLabels = chartHocKyLabels,
            ChartHocKyData = chartHocKyData,
            ChartLopLabels = chartLopLabels,
            ChartLopTyLeData = chartLopTyLeData,

            RecentPayments = recentPayments,
            RecentLogs = recentLogs
        };
    }

    // =========================================================================
    // IV. QUẢN LÝ KHOẢN THU (KHOANTHU)
    // =========================================================================

    public async Task<AdminKhoanThuListViewModel> GetKhoanThuListAsync(int? hocKyId, string? trangThai, string? search)
    {
        var query = _db.DotThuHocPhis
            .Include(d => d.HocKy)
            .Include(d => d.HoaDonHocPhis)
            .AsQueryable();

        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            query = query.Where(d => d.HocKyId == hocKyId.Value);
        }
        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(d => d.TrangThai == trangThai);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.MaKhoanThu.Contains(search) || d.TenKhoanThu.Contains(search));
        }

        var list = await query.OrderByDescending(d => d.NgayTao).ToListAsync();

        return new AdminKhoanThuListViewModel
        {
            DanhSachDotThu = list,
            DanhSachHocKy = await _db.HocKies.OrderByDescending(h => h.NgayBatDau).ToListAsync(),
            HocKyId = hocKyId,
            TrangThai = trangThai,
            Search = search,
            TongSoKhoanThu = list.Count,
            TongSoTienKeHoach = list.Sum(d => d.SoTien * (d.HoaDonHocPhis.Count > 0 ? d.HoaDonHocPhis.Count : 1))
        };
    }

    public async Task<AdminTaoKhoanThuViewModel> GetTaoKhoanThuModelAsync()
    {
        return new AdminTaoKhoanThuViewModel
        {
            MaKhoanThu = $"KT{DateTime.UtcNow:yyyyMMddHHmm}",
            TenKhoanThu = "Học phí học kỳ mới",
            SoTien = 6500000m,
            NgayBatDau = DateTime.Today,
            HanThanhToan = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            DanhSachHocKy = await _db.HocKies.OrderByDescending(h => h.NgayBatDau).ToListAsync(),
            DanhSachNganh = await _db.Nganhs.OrderBy(n => n.TenNganh).ToListAsync(),
            DanhSachLop = await _db.Lops.OrderBy(l => l.TenLop).ToListAsync()
        };
    }

    public async Task<(bool Success, string Message, int Id)> TaoKhoanThuAsync(AdminTaoKhoanThuViewModel model, string nguoiTao)
    {
        if (await _db.DotThuHocPhis.AnyAsync(d => d.MaKhoanThu == model.MaKhoanThu))
        {
            return (false, "Mã khoản thu đã tồn tại trên hệ thống.", 0);
        }

        var dotThu = new DotThuHocPhi
        {
            MaKhoanThu = model.MaKhoanThu.Trim(),
            TenKhoanThu = model.TenKhoanThu.Trim(),
            MoTa = model.MoTa,
            HocKyId = model.HocKyId,
            SoTien = model.SoTien,
            NgayBatDau = model.NgayBatDau,
            HanThanhToan = model.HanThanhToan,
            DoiTuongApDung = model.DoiTuongApDung,
            GiaTriApDung = model.GiaTriApDung,
            TrangThai = "ACTIVE",
            NguoiTao = nguoiTao,
            NgayTao = DateTime.UtcNow
        };

        _db.DotThuHocPhis.Add(dotThu);
        await _db.SaveChangesAsync();

        int sinhVienCount = 0;

        // Nếu bật tự động phát hành hóa đơn học phí cho sinh viên
        if (model.PhatHanhHoaDon)
        {
            var studentQuery = _db.SinhViens.AsQueryable();

            if (model.DoiTuongApDung == "THEO_LOP" && int.TryParse(model.GiaTriApDung, out var lopId))
            {
                studentQuery = studentQuery.Where(s => s.LopId == lopId);
            }
            else if (model.DoiTuongApDung == "THEO_NGANH" && int.TryParse(model.GiaTriApDung, out var nganhId))
            {
                studentQuery = studentQuery.Where(s => s.Lop != null && s.Lop.NganhId == nganhId);
            }
            else if (model.DoiTuongApDung == "THEO_SINH_VIEN" && !string.IsNullOrWhiteSpace(model.GiaTriApDung))
            {
                var msvList = model.GiaTriApDung.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ToList();
                studentQuery = studentQuery.Where(s => msvList.Contains(s.MaSinhVien));
            }

            var eligibleStudents = await studentQuery.ToListAsync();
            foreach (var sv in eligibleStudents)
            {
                var soHd = $"HD-{dotThu.MaKhoanThu}-{sv.MaSinhVien}";
                if (!await _db.HoaDonHocPhis.AnyAsync(h => h.SoHoaDon == soHd))
                {
                    _db.HoaDonHocPhis.Add(new HoaDonHocPhi
                    {
                        SoHoaDon = soHd,
                        SinhVienId = sv.Id,
                        HocKyId = model.HocKyId,
                        TenKhoanThu = model.TenKhoanThu,
                        SoTien = model.SoTien,
                        DaThanhToan = 0,
                        NgayLap = DateTime.UtcNow,
                        HanThanhToan = model.HanThanhToan,
                        TrangThai = "CHUA_THANH_TOAN",
                        GhiChu = $"Khoản thu {dotThu.MaKhoanThu} tạo tự động cho SV {sv.MaSinhVien}",
                        DotThuId = dotThu.Id
                    });
                    sinhVienCount++;
                }
            }
            await _db.SaveChangesAsync();
        }

        await GhiNhatKyAuditAsync("TAO_KHOAN_THU", "DotThuHocPhi", dotThu.Id.ToString(), nguoiTao, 
            $"Tạo khoản thu {dotThu.MaKhoanThu} - {dotThu.TenKhoanThu}. Phát hành {sinhVienCount} hóa đơn sinh viên.");

        return (true, $"Tạo khoản thu thành công. Đã phát hành hóa đơn cho {sinhVienCount} sinh viên.", dotThu.Id);
    }

    public async Task<AdminChiTietKhoanThuViewModel?> GetChiTietKhoanThuAsync(int id)
    {
        var dotThu = await _db.DotThuHocPhis
            .Include(d => d.HocKy)
                .ThenInclude(hk => hk!.NamHoc)
            .Include(d => d.HoaDonHocPhis)
                .ThenInclude(h => h.SinhVien)
                    .ThenInclude(s => s.Lop)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dotThu == null) return null;

        var hds = dotThu.HoaDonHocPhis.ToList();
        var tongPhaiThu = hds.Sum(h => h.SoTien);
        var tongDaThu = hds.Sum(h => h.DaThanhToan);
        var daDongCount = hds.Count(h => h.DaThanhToan >= h.SoTien && h.SoTien > 0);

        // Kiểm tra xem đã có giao dịch Paid nào chưa
        var hasPaid = await _db.Payments
            .AnyAsync(p => p.Status == "PAID" && hds.Select(h => h.Id).Contains(p.HoaDonHocPhiId));

        return new AdminChiTietKhoanThuViewModel
        {
            DotThu = dotThu,
            TongSinhVien = hds.Count,
            SinhVienDaDong = daDongCount,
            SinhVienChuaDong = Math.Max(0, hds.Count - daDongCount),
            TongPhaiThu = tongPhaiThu,
            TongDaThu = tongDaThu,
            DanhSachHoaDon = hds,
            CoTheSuaSoTien = !hasPaid
        };
    }

    public async Task<(bool Success, string Message)> NgungKhoanThuAsync(int id, string nguoiThucHien)
    {
        var dotThu = await _db.DotThuHocPhis.FindAsync(id);
        if (dotThu == null) return (false, "Không tìm thấy khoản thu.");

        dotThu.TrangThai = "STOPPED";
        await _db.SaveChangesAsync();

        await GhiNhatKyAuditAsync("NGUNG_KHOAN_THU", "DotThuHocPhi", id.ToString(), nguoiThucHien, "Ngừng áp dụng khoản thu");
        return (true, "Đã ngừng áp dụng khoản thu thành công.");
    }

    public async Task<(bool Success, string Message)> CapNhatKhoanThuAsync(int id, string tenKhoanThu, string? moTa, DateOnly? hanThanhToan, decimal? soTienMoi, string nguoiThucHien)
    {
        var dotThu = await _db.DotThuHocPhis
            .Include(d => d.HoaDonHocPhis)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (dotThu == null) return (false, "Không tìm thấy khoản thu.");

        dotThu.TenKhoanThu = tenKhoanThu.Trim();
        dotThu.MoTa = moTa;
        dotThu.HanThanhToan = hanThanhToan;

        // Nếu có điều chỉnh số tiền, bắt buộc kiểm tra xem đã phát sinh giao dịch Paid chưa
        if (soTienMoi.HasValue && soTienMoi.Value > 0 && soTienMoi.Value != dotThu.SoTien)
        {
            var hdIds = dotThu.HoaDonHocPhis.Select(h => h.Id).ToList();
            var hasPaid = await _db.Payments.AnyAsync(p => p.Status == "PAID" && hdIds.Contains(p.HoaDonHocPhiId));
            if (hasPaid)
            {
                return (false, "Không thể sửa số tiền vì khoản thu này đã có sinh viên thanh toán thành công!");
            }

            var oldAmount = dotThu.SoTien;
            dotThu.SoTien = soTienMoi.Value;

            // Đồng bộ sang các hóa đơn chưa đóng
            foreach (var hd in dotThu.HoaDonHocPhis.Where(h => h.DaThanhToan == 0))
            {
                hd.SoTien = soTienMoi.Value;
            }

            await GhiNhatKyAuditAsync("SUA_KHOAN_THU", "DotThuHocPhi", id.ToString(), nguoiThucHien,
                $"Cập nhật số tiền từ {oldAmount:N0}đ sang {soTienMoi.Value:N0}đ", null, oldAmount.ToString(), soTienMoi.Value.ToString());
        }

        await _db.SaveChangesAsync();
        return (true, "Đã cập nhật thông tin khoản thu thành công.");
    }

    // =========================================================================
    // V. DANH SÁCH GIAO DỊCH & CHI TIẾT GIAO DỊCH
    // =========================================================================

    public async Task<AdminGiaoDichListViewModel> GetGiaoDichListAsync(
        int page, int pageSize, string? search, string? status, string? bankId, string? tuNgay, string? denNgay)
    {
        var query = _db.Payments
            .Include(p => p.SinhVien)
                .ThenInclude(s => s.Lop)
            .Include(p => p.HoaDonHocPhi)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.TransactionCode.Contains(s) 
                                  || p.SinhVien.MaSinhVien.Contains(s) 
                                  || p.SinhVien.HoTen.Contains(s)
                                  || (p.BankTransactionId != null && p.BankTransactionId.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(bankId))
        {
            query = query.Where(p => p.PaymentMethod.Contains(bankId));
        }

        if (!string.IsNullOrWhiteSpace(tuNgay) && DateTime.TryParse(tuNgay, out var dTu))
        {
            query = query.Where(p => p.CreatedAt >= dTu);
        }

        if (!string.IsNullOrWhiteSpace(denNgay) && DateTime.TryParse(denNgay, out var dDen))
        {
            var dDenEnd = dDen.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.CreatedAt <= dDenEnd);
        }

        var totalItems = await query.CountAsync();
        var tongTienFilter = await query.SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new AdminGiaoDichListViewModel
        {
            DanhSachGiaoDich = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TongTienFilter = tongTienFilter,
            Search = search,
            Status = status,
            BankId = bankId,
            TuNgay = tuNgay,
            DenNgay = denNgay
        };
    }

    public async Task<AdminChiTietGiaoDichViewModel?> GetChiTietGiaoDichAsync(long id)
    {
        var payment = await _db.Payments
            .Include(p => p.SinhVien)
                .ThenInclude(s => s.Lop)
                    .ThenInclude(l => l!.Nganh)
            .Include(p => p.HoaDonHocPhi)
                .ThenInclude(h => h.HocKy)
                    .ThenInclude(hk => hk!.NamHoc)
            .Include(p => p.BankTransaction)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null) return null;

        var webhookLogs = await _db.PaymentWebhookLogs
            .Where(w => w.EventId == payment.TransactionCode || (payment.BankTransactionId != null && w.EventId == payment.BankTransactionId))
            .OrderByDescending(w => w.ReceivedAt)
            .ToListAsync();

        var auditLogs = await _db.NhatKyThanhToans
            .Where(a => a.DoiTuong == "Payment" && a.KhoaChinh == payment.Id.ToString())
            .OrderByDescending(a => a.ThoiGian)
            .ToListAsync();

        return new AdminChiTietGiaoDichViewModel
        {
            Payment = payment,
            BankTransaction = payment.BankTransaction,
            WebhookLogs = webhookLogs,
            AuditLogs = auditLogs
        };
    }

    public async Task<(bool Success, string Message)> XacNhanGiaoDichThuCongAsync(
        long paymentId, string lyDo, string? maGiaoDichNganHang, string nguoiThucHien)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
        {
            return (false, "Bắt buộc phải nhập lý do xác nhận thu tiền.");
        }

        using var trans = await _db.Database.BeginTransactionAsync();
        try
        {
            var payment = await _db.Payments
                .Include(p => p.HoaDonHocPhi)
                .Include(p => p.SinhVien)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return (false, "Không tìm thấy giao dịch.");
            }

            if (payment.Status == "PAID")
            {
                return (false, "Giao dịch này đã được xác nhận thanh toán trước đó.");
            }

            var trangThaiTruoc = payment.Status;
            payment.Status = "PAID";
            payment.PaidAt = DateTime.UtcNow;
            payment.IsAutoVerified = false; // Đánh dấu là duyệt thủ công bởi Admin
            if (!string.IsNullOrWhiteSpace(maGiaoDichNganHang))
            {
                payment.BankTransactionId = maGiaoDichNganHang.Trim();
            }

            // Cập nhật hóa đơn học phí
            if (payment.HoaDonHocPhi != null)
            {
                payment.HoaDonHocPhi.DaThanhToan += payment.Amount;
                if (payment.HoaDonHocPhi.ConPhaiDong <= 0)
                {
                    payment.HoaDonHocPhi.TrangThai = "DA_THANH_TOAN";
                }
                else
                {
                    payment.HoaDonHocPhi.TrangThai = "THANH_TOAN_MOT_PHAN";
                }
            }

            await _db.SaveChangesAsync();

            await GhiNhatKyAuditAsync("XAC_NHAN_THANH_TOAN", "Payment", payment.Id.ToString(), nguoiThucHien,
                lyDo, maGiaoDichNganHang, trangThaiTruoc, "PAID",
                $"Admin {nguoiThucHien} duyệt thủ công giao dịch {payment.TransactionCode}, số tiền {payment.Amount:N0}đ");

            await trans.CommitAsync();
            return (true, $"Đã xác nhận thanh toán thành công cho giao dịch {payment.TransactionCode}.");
        }
        catch (Exception ex)
        {
            await trans.RollbackAsync();
            _logger.LogError(ex, "Lỗi khi Admin xác nhận giao dịch {Id}", paymentId);
            return (false, "Lỗi hệ thống khi xác nhận giao dịch: " + ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> TuChoiGiaoDichAsync(
        long paymentId, string lyDo, string nguoiThucHien)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
        {
            return (false, "Bắt buộc phải nhập lý do từ chối giao dịch.");
        }

        var payment = await _db.Payments.FindAsync(paymentId);
        if (payment == null)
        {
            return (false, "Không tìm thấy giao dịch.");
        }

        if (payment.Status == "PAID")
        {
            return (false, "Không thể từ chối giao dịch đã thanh toán thành công.");
        }

        var trangThaiTruoc = payment.Status;
        payment.Status = "CANCELLED";
        await _db.SaveChangesAsync();

        await GhiNhatKyAuditAsync("TU_CHOI_GIAO_DICH", "Payment", payment.Id.ToString(), nguoiThucHien,
            lyDo, null, trangThaiTruoc, "CANCELLED",
            $"Admin {nguoiThucHien} từ chối/hủy giao dịch {payment.TransactionCode}");

        return (true, $"Đã từ chối giao dịch {payment.TransactionCode}.");
    }

    // =========================================================================
    // VI. QUẢN LÝ CÔNG NỢ SINH VIÊN
    // =========================================================================

    public async Task<AdminCongNoListViewModel> GetCongNoListAsync(
        int page, int pageSize, string? search, int? lopId, int? nganhId, int? hocKyId, bool chiSinhVienNo)
    {
        var svQuery = _db.SinhViens
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .AsQueryable();

        if (lopId.HasValue && lopId.Value > 0)
        {
            svQuery = svQuery.Where(s => s.LopId == lopId.Value);
        }

        if (nganhId.HasValue && nganhId.Value > 0)
        {
            svQuery = svQuery.Where(s => s.Lop != null && s.Lop.NganhId == nganhId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            svQuery = svQuery.Where(sv => sv.MaSinhVien.Contains(s) || sv.HoTen.Contains(s));
        }

        var allMatchingStudents = await svQuery.ToListAsync();
        var studentIds = allMatchingStudents.Select(s => s.Id).ToList();

        var hdQuery = _db.HoaDonHocPhis.Where(h => studentIds.Contains(h.SinhVienId));
        if (hocKyId.HasValue && hocKyId.Value > 0)
        {
            hdQuery = hdQuery.Where(h => h.HocKyId == hocKyId.Value);
        }

        var invoices = await hdQuery.ToListAsync();

        var congNoList = new List<SinhVienCongNoItem>();
        foreach (var sv in allMatchingStudents)
        {
            var svHds = invoices.Where(h => h.SinhVienId == sv.Id).ToList();
            var tongHp = svHds.Sum(h => h.SoTien);
            var daThu = svHds.Sum(h => h.DaThanhToan);
            var conNo = Math.Max(0, tongHp - daThu);
            var chuaThanhToanCount = svHds.Count(h => h.ConPhaiDong > 0);

            if (chiSinhVienNo && conNo <= 0) continue;

            congNoList.Add(new SinhVienCongNoItem
            {
                SinhVienId = sv.Id,
                MaSinhVien = sv.MaSinhVien,
                HoTen = sv.HoTen,
                TenLop = sv.Lop?.TenLop ?? "Chưa xếp lớp",
                TenNganh = sv.Lop?.Nganh?.TenNganh ?? "",
                TongHocPhi = tongHp,
                DaThanhToan = daThu,
                SoKhoanChuaThanhToan = chuaThanhToanCount
            });
        }

        var totalItems = congNoList.Count;
        var pagedItems = congNoList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new AdminCongNoListViewModel
        {
            DanhSachCongNo = pagedItems,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TongPhaiThu = congNoList.Sum(c => c.TongHocPhi),
            TongDaThu = congNoList.Sum(c => c.DaThanhToan),
            TongConNo = congNoList.Sum(c => c.ConNo),
            SoSinhVienConNo = congNoList.Count(c => c.ConNo > 0),
            Search = search,
            LopId = lopId,
            NganhId = nganhId,
            HocKyId = hocKyId,
            ChiSinhVienNo = chiSinhVienNo,
            DanhSachLop = await _db.Lops.OrderBy(l => l.TenLop).ToListAsync(),
            DanhSachNganh = await _db.Nganhs.OrderBy(n => n.TenNganh).ToListAsync(),
            DanhSachHocKy = await _db.HocKies.OrderByDescending(h => h.NgayBatDau).ToListAsync()
        };
    }

    public async Task<AdminChiTietCongNoViewModel?> GetChiTietCongNoAsync(int sinhVienId)
    {
        var sv = await _db.SinhViens
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .FirstOrDefaultAsync(s => s.Id == sinhVienId);

        if (sv == null) return null;

        var invoices = await _db.HoaDonHocPhis
            .Include(h => h.HocKy)
            .Where(h => h.SinhVienId == sinhVienId)
            .OrderByDescending(h => h.NgayLap)
            .ToListAsync();

        var payments = await _db.Payments
            .Where(p => p.SinhVienId == sinhVienId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var auditLogs = await _db.NhatKyThanhToans
            .Where(a => a.DoiTuong == "HoaDonHocPhi" && invoices.Select(h => h.Id.ToString()).Contains(a.KhoaChinh ?? ""))
            .OrderByDescending(a => a.ThoiGian)
            .ToListAsync();

        var tongHocPhi = invoices.Sum(h => h.SoTien);
        var daThanhToan = invoices.Sum(h => h.DaThanhToan);

        return new AdminChiTietCongNoViewModel
        {
            SinhVien = sv,
            TongHocPhi = tongHocPhi,
            DaThanhToan = daThanhToan,
            DanhSachHoaDon = invoices,
            LichSuThanhToan = payments,
            NhatKyDieuChinh = auditLogs
        };
    }

    public async Task<(bool Success, string Message)> DieuChinhCongNoAsync(DieuChinhCongNoRequest request, string nguoiThucHien)
    {
        if (string.IsNullOrWhiteSpace(request.LyDo))
        {
            return (false, "Bắt buộc phải nhập lý do điều chỉnh công nợ.");
        }

        var hd = await _db.HoaDonHocPhis.FindAsync(request.HoaDonId);
        if (hd == null) return (false, "Không tìm thấy hóa đơn học phí.");

        var oldSoTien = hd.SoTien;
        hd.SoTien = request.SoTienMoi;
        if (hd.DaThanhToan >= hd.SoTien)
        {
            hd.TrangThai = "DA_THANH_TOAN";
        }
        else if (hd.DaThanhToan > 0)
        {
            hd.TrangThai = "THANH_TOAN_MOT_PHAN";
        }
        else
        {
            hd.TrangThai = "CHUA_THANH_TOAN";
        }

        await _db.SaveChangesAsync();

        await GhiNhatKyAuditAsync("DIEU_CHINH_CONG_NO", "HoaDonHocPhi", hd.Id.ToString(), nguoiThucHien,
            request.LyDo, request.BangChung, oldSoTien.ToString(), request.SoTienMoi.ToString(),
            $"Điều chỉnh số tiền hóa đơn {hd.SoHoaDon} từ {oldSoTien:N0}đ sang {request.SoTienMoi:N0}đ");

        return (true, "Đã điều chỉnh công nợ thành công và lưu nhật ký kiểm toán.");
    }

    // =========================================================================
    // VII. ĐỐI SOÁT NGÂN HÀNG VIETCOMBANK
    // =========================================================================

    public async Task<AdminDoiSoatViewModel> GetDoiSoatListAsync(string? ketQuaFilter, string? tuNgay, string? denNgay, string? search)
    {
        var bankTxnQuery = _db.BankTransactions
            .Include(b => b.Payment)
                .ThenInclude(p => p!.SinhVien)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            bankTxnQuery = bankTxnQuery.Where(b => b.BankTransactionId.Contains(s) || b.TransactionContent.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(tuNgay) && DateTime.TryParse(tuNgay, out var dTu))
        {
            bankTxnQuery = bankTxnQuery.Where(b => b.TransactionDate >= dTu);
        }

        if (!string.IsNullOrWhiteSpace(denNgay) && DateTime.TryParse(denNgay, out var dDen))
        {
            var dDenEnd = dDen.Date.AddDays(1).AddTicks(-1);
            bankTxnQuery = bankTxnQuery.Where(b => b.TransactionDate <= dDenEnd);
        }

        var bankTxns = await bankTxnQuery.OrderByDescending(b => b.TransactionDate).ToListAsync();

        var doiSoatItems = new List<DoiSoatItemViewModel>();
        foreach (var b in bankTxns)
        {
            string ketQua = "MATCHED";
            string? ghiChu = null;

            if (b.Payment == null)
            {
                // Thử tìm theo nội dung
                var match = Regex.Match(b.TransactionContent, @"(HP\d{6}[A-Za-z0-9]{6})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    ketQua = "UNMATCHED_NO_ORDER";
                    ghiChu = $"Có mã '{match.Value}' nhưng không tìm thấy đơn thanh toán trong DB.";
                }
                else
                {
                    ketQua = "CONTENT_MISMATCH";
                    ghiChu = "Nội dung chuyển khoản không đúng chuẩn HP <MaSV> <MaGD>.";
                }
            }
            else
            {
                if (b.Amount != b.Payment.Amount)
                {
                    ketQua = "AMOUNT_MISMATCH";
                    ghiChu = $"Ngân hàng báo {b.Amount:N0}đ, đơn yêu cầu {b.Payment.Amount:N0}đ.";
                }
                else if (b.Payment.Status != "PAID")
                {
                    ketQua = "MANUAL_REVIEW";
                    ghiChu = $"Đã link với đơn {b.Payment.TransactionCode} nhưng trạng thái hiện tại là {b.Payment.Status}.";
                }
            }

            if (!string.IsNullOrWhiteSpace(ketQuaFilter) && ketQua != ketQuaFilter)
            {
                continue;
            }

            doiSoatItems.Add(new DoiSoatItemViewModel
            {
                BankTransactionDbId = b.Id,
                BankTransactionId = b.BankTransactionId,
                TransactionContent = b.TransactionContent,
                AmountBank = b.Amount,
                TransactionDate = b.TransactionDate,
                PaymentTransactionCode = b.Payment?.TransactionCode,
                AmountInternal = b.Payment?.Amount,
                InternalStatus = b.Payment?.Status,
                StudentInfo = b.Payment?.SinhVien != null ? $"{b.Payment.SinhVien.MaSinhVien} - {b.Payment.SinhVien.HoTen}" : null,
                KetQuaDoiSoat = ketQua,
                GhiChu = ghiChu,
                DaXuLy = b.Payment != null && b.Payment.Status == "PAID"
            });
        }

        return new AdminDoiSoatViewModel
        {
            DanhSachDoiSoat = doiSoatItems,
            TongGiaoDichNganHang = bankTxns.Count,
            TongTienBaoCo = bankTxns.Sum(b => b.Amount),
            SoGiaoDichKhop = doiSoatItems.Count(d => d.KetQuaDoiSoat == "MATCHED"),
            SoGiaoDichBatThuong = doiSoatItems.Count(d => d.KetQuaDoiSoat != "MATCHED"),
            KetQuaFilter = ketQuaFilter,
            TuNgay = tuNgay,
            DenNgay = denNgay,
            Search = search
        };
    }

    public async Task<(bool Success, string Message)> XuLyBatThuongDoiSoatAsync(XuLyBatThuongRequest request, string nguoiThucHien)
    {
        if (string.IsNullOrWhiteSpace(request.LyDo))
        {
            return (false, "Bắt buộc phải nhập lý do xử lý bất thường.");
        }

        var bankTxn = await _db.BankTransactions.FindAsync(request.BankTransactionDbId);
        if (bankTxn == null) return (false, "Không tìm thấy giao dịch ngân hàng.");

        if (request.HanhDong == "XAC_NHAN_KHOP" && !string.IsNullOrWhiteSpace(request.PaymentCode))
        {
            var payment = await _db.Payments
                .Include(p => p.HoaDonHocPhi)
                .FirstOrDefaultAsync(p => p.TransactionCode == request.PaymentCode.Trim());

            if (payment == null) return (false, "Không tìm thấy giao dịch nội bộ cần gán.");

            bankTxn.PaymentId = payment.Id;
            payment.Status = "PAID";
            payment.PaidAt = DateTime.UtcNow;
            payment.BankTransactionId = bankTxn.BankTransactionId;
            payment.IsAutoVerified = false; // Đánh dấu là duyệt thủ công qua đối soát

            if (payment.HoaDonHocPhi != null)
            {
                payment.HoaDonHocPhi.DaThanhToan += payment.Amount;
                if (payment.HoaDonHocPhi.ConPhaiDong <= 0)
                {
                    payment.HoaDonHocPhi.TrangThai = "DA_THANH_TOAN";
                }
            }

            await _db.SaveChangesAsync();

            await GhiNhatKyAuditAsync("XAC_NHAN_THU_CONG", "BankTransaction", bankTxn.Id.ToString(), nguoiThucHien,
                request.LyDo, request.BangChung, "UNMATCHED", "MATCHED", $"Gán giao dịch NH {bankTxn.BankTransactionId} với đơn {payment.TransactionCode}");

            return (true, "Đã đối soát khớp và xác nhận thanh toán thành công.");
        }
        else
        {
            await GhiNhatKyAuditAsync("XU_LY_BAT_THUONG", "BankTransaction", bankTxn.Id.ToString(), nguoiThucHien,
                request.LyDo, request.BangChung, null, request.HanhDong, $"Xử lý: {request.HanhDong}");

            return (true, "Đã ghi nhận kết quả xử lý đối soát.");
        }
    }

    // =========================================================================
    // VIII. GHI NHẬT KÝ KIỂM TOÁN (AUDIT LOG)
    // =========================================================================

    public async Task GhiNhatKyAuditAsync(
        string hanhDong, string doiTuong, string? khoaChinh, string nguoiThucHien, string lyDo, 
        string? bangChung = null, string? trangThaiTruoc = null, string? trangThaiSau = null, string? chiTiet = null)
    {
        try
        {
            var audit = new NhatKyThanhToan
            {
                HanhDong = hanhDong,
                DoiTuong = doiTuong,
                KhoaChinh = khoaChinh,
                NguoiThucHien = nguoiThucHien,
                ThoiGian = DateTime.UtcNow,
                LyDo = lyDo,
                BangChungDoiChieu = bangChung,
                TrangThaiTruoc = trangThaiTruoc,
                TrangThaiSau = trangThaiSau,
                ChiTiet = chiTiet
            };

            _db.NhatKyThanhToans.Add(audit);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ghi nhật ký kiểm toán NhatKyThanhToan.");
        }
    }

    // =========================================================================
    // IX. HELPER TỰ ĐỘNG KHỞI TẠO HÓA ĐƠN MẪU
    // =========================================================================

    private async Task EnsureHocPhiInitializedAsync(SinhVien student)
    {
        try
        {
            var count = await _db.HoaDonHocPhis.CountAsync(h => h.SinhVienId == student.Id);
            if (count == 0)
            {
                var hocKy = await _db.HocKies.FirstOrDefaultAsync(h => h.TrangThai == "DANG_MO") 
                         ?? await _db.HocKies.OrderByDescending(h => h.NgayBatDau).FirstOrDefaultAsync();

                if (hocKy != null)
                {
                    var tinChi = await _db.DangKyHocPhans
                        .Where(dk => dk.SinhVienId == student.Id && dk.TrangThai == "ENROLLED")
                        .SumAsync(dk => (int?)dk.LopHocPhan!.MonHoc!.SoTinChi) ?? 14;

                    if (tinChi == 0) tinChi = 14;
                    decimal donGiaTinChi = 450000m;
                    decimal tongTien = tinChi * donGiaTinChi;

                    _db.HoaDonHocPhis.Add(new HoaDonHocPhi
                    {
                        SoHoaDon = $"HD{DateTime.UtcNow:yyyyMM}{student.Id:D4}",
                        SinhVienId = student.Id,
                        HocKyId = hocKy.HocKyId,
                        TenKhoanThu = $"Học phí {hocKy.TenHocKy} ({tinChi} tín chỉ)",
                        SoTien = tongTien,
                        DaThanhToan = 0,
                        NgayLap = DateTime.UtcNow,
                        HanThanhToan = hocKy.NgayKetThuc,
                        TrangThai = "CHUA_THANH_TOAN",
                        GhiChu = $"Học phí chính khóa đào tạo tín chỉ: {tinChi} tín chỉ x 450.000đ/TC."
                    });

                    await _db.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể tự khởi tạo hóa đơn học phí mẫu cho sinh viên {Id}.", student.Id);
        }
    }
}
