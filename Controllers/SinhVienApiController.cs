using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

/// <summary>
/// RESTful Web API Controller quản lý Sinh viên (Chuẩn mô hình RESTful Service)
/// Các phương thức HTTP hỗ trợ: GET, POST, PUT, DELETE
/// Đường dẫn gốc: /api/sinhvien
/// </summary>
[ApiController]
[Route("api/sinhvien")]
[Produces("application/json")]
public class SinhVienApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SinhVienApiController> _logger;

    public SinhVienApiController(ApplicationDbContext db, ILogger<SinhVienApiController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/sinhvien: Lấy danh sách sinh viên (Hỗ trợ tìm kiếm từ khóa, lọc theo lớp, trạng thái và phân trang)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SinhVienDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] int? lopId,
        [FromQuery] string? trangThai,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.SinhViens
            .AsNoTracking()
            .Include(s => s.Lop)
                .ThenInclude(l => l!.Nganh)
            .Include(s => s.Diems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(s => s.MaSinhVien.ToLower().Contains(kw) || s.HoTen.ToLower().Contains(kw) || (s.Email != null && s.Email.ToLower().Contains(kw)));
        }

        if (lopId.HasValue)
        {
            query = query.Where(s => s.LopId == lopId.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            query = query.Where(s => s.TrangThai == trangThai);
        }

        var totalCount = await query.CountAsync();

        var list = await query
            .OrderBy(s => s.MaSinhVien)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SinhVienDto
            {
                Id = s.Id,
                MaSinhVien = s.MaSinhVien,
                HoTen = s.HoTen,
                NgaySinh = s.NgaySinh.HasValue ? s.NgaySinh.Value.ToString("yyyy-MM-dd") : null,
                GioiTinh = s.GioiTinh,
                Email = s.Email,
                SoDienThoai = s.SoDienThoai,
                DiaChi = s.DiaChi,
                LopId = s.LopId,
                TenLop = s.Lop != null ? s.Lop.TenLop : null,
                Nganh = s.Lop != null && s.Lop.Nganh != null ? s.Lop.Nganh.TenNganh : null,
                NienKhoa = s.NienKhoa,
                TrangThai = s.TrangThai,
                HinhAnh = s.HinhAnh,
                Gpa = s.Gpa
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SinhVienDto>>.Paged(list, totalCount, page, pageSize, $"Tìm thấy {totalCount} sinh viên"));
    }

    /// <summary>
    /// GET /api/sinhvien/{id}: Lấy thông tin chi tiết của 1 sinh viên theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SinhVienDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _db.SinhViens
            .AsNoTracking()
            .Include(x => x.Lop).ThenInclude(l => l!.Nganh)
            .Include(x => x.Diems)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (s == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Không tìm thấy sinh viên có ID = {id}"));
        }

        var dto = new SinhVienDto
        {
            Id = s.Id,
            MaSinhVien = s.MaSinhVien,
            HoTen = s.HoTen,
            NgaySinh = s.NgaySinh.HasValue ? s.NgaySinh.Value.ToString("yyyy-MM-dd") : null,
            GioiTinh = s.GioiTinh,
            Email = s.Email,
            SoDienThoai = s.SoDienThoai,
            DiaChi = s.DiaChi,
            LopId = s.LopId,
            TenLop = s.Lop?.TenLop,
            Nganh = s.Lop?.Nganh?.TenNganh,
            NienKhoa = s.NienKhoa,
            TrangThai = s.TrangThai,
            HinhAnh = s.HinhAnh,
            Gpa = s.Gpa
        };

        return Ok(ApiResponse<SinhVienDto>.Ok(dto, "Lấy thông tin sinh viên thành công"));
    }

    /// <summary>
    /// GET /api/sinhvien/masv/{maSv}: Tra cứu sinh viên theo Mã sinh viên
    /// </summary>
    [HttpGet("masv/{maSv}")]
    [ProducesResponseType(typeof(ApiResponse<SinhVienDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByMaSv(string maSv)
    {
        var s = await _db.SinhViens
            .AsNoTracking()
            .Include(x => x.Lop).ThenInclude(l => l!.Nganh)
            .Include(x => x.Diems)
            .FirstOrDefaultAsync(x => x.MaSinhVien.ToLower() == maSv.Trim().ToLower());

        if (s == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Không tìm thấy sinh viên có mã '{maSv}'"));
        }

        var dto = new SinhVienDto
        {
            Id = s.Id,
            MaSinhVien = s.MaSinhVien,
            HoTen = s.HoTen,
            NgaySinh = s.NgaySinh.HasValue ? s.NgaySinh.Value.ToString("yyyy-MM-dd") : null,
            GioiTinh = s.GioiTinh,
            Email = s.Email,
            SoDienThoai = s.SoDienThoai,
            DiaChi = s.DiaChi,
            LopId = s.LopId,
            TenLop = s.Lop?.TenLop,
            Nganh = s.Lop?.Nganh?.TenNganh,
            NienKhoa = s.NienKhoa,
            TrangThai = s.TrangThai,
            HinhAnh = s.HinhAnh,
            Gpa = s.Gpa
        };

        return Ok(ApiResponse<SinhVienDto>.Ok(dto, "Tra cứu sinh viên thành công"));
    }

    /// <summary>
    /// POST /api/sinhvien: Thêm mới 1 sinh viên (Chuẩn RESTful: trả về HTTP 201 Created kèm Location header)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SinhVienDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSinhVienDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<object>.Fail($"Dữ liệu không hợp lệ: {errors}"));
        }

        var maSvTrim = dto.MaSinhVien.Trim();
        var exists = await _db.SinhViens.AnyAsync(s => s.MaSinhVien.ToLower() == maSvTrim.ToLower());
        if (exists)
        {
            return Conflict(ApiResponse<object>.Fail($"Mã sinh viên '{maSvTrim}' đã tồn tại trong hệ thống"));
        }

        if (dto.LopId.HasValue)
        {
            var lopExists = await _db.Lops.AnyAsync(l => l.Id == dto.LopId.Value);
            if (!lopExists)
            {
                return BadRequest(ApiResponse<object>.Fail($"Lớp học có ID = {dto.LopId.Value} không tồn tại"));
            }
        }

        var sinhVien = new SinhVien
        {
            MaSinhVien = maSvTrim,
            HoTen = dto.HoTen.Trim(),
            NgaySinh = dto.NgaySinh,
            GioiTinh = dto.GioiTinh,
            Email = dto.Email?.Trim(),
            SoDienThoai = dto.SoDienThoai?.Trim(),
            DiaChi = dto.DiaChi?.Trim(),
            LopId = dto.LopId,
            NienKhoa = dto.NienKhoa?.Trim(),
            TrangThai = string.IsNullOrWhiteSpace(dto.TrangThai) ? "Đang học" : dto.TrangThai.Trim()
        };

        _db.SinhViens.Add(sinhVien);
        await _db.SaveChangesAsync();

        var resultDto = new SinhVienDto
        {
            Id = sinhVien.Id,
            MaSinhVien = sinhVien.MaSinhVien,
            HoTen = sinhVien.HoTen,
            NgaySinh = sinhVien.NgaySinh.HasValue ? sinhVien.NgaySinh.Value.ToString("yyyy-MM-dd") : null,
            GioiTinh = sinhVien.GioiTinh,
            Email = sinhVien.Email,
            SoDienThoai = sinhVien.SoDienThoai,
            DiaChi = sinhVien.DiaChi,
            LopId = sinhVien.LopId,
            NienKhoa = sinhVien.NienKhoa,
            TrangThai = sinhVien.TrangThai
        };

        return CreatedAtAction(nameof(GetById), new { id = sinhVien.Id }, ApiResponse<SinhVienDto>.Ok(resultDto, "Thêm mới sinh viên thành công"));
    }

    /// <summary>
    /// PUT /api/sinhvien/{id}: Cập nhật thông tin sinh viên theo ID (Chuẩn RESTful: HTTP 200 OK)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SinhVienDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSinhVienDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(ApiResponse<object>.Fail($"Dữ liệu không hợp lệ: {errors}"));
        }

        var sinhVien = await _db.SinhViens.FindAsync(id);
        if (sinhVien == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Không tìm thấy sinh viên có ID = {id} để cập nhật"));
        }

        if (dto.LopId.HasValue)
        {
            var lopExists = await _db.Lops.AnyAsync(l => l.Id == dto.LopId.Value);
            if (!lopExists)
            {
                return BadRequest(ApiResponse<object>.Fail($"Lớp học có ID = {dto.LopId.Value} không tồn tại"));
            }
        }

        sinhVien.HoTen = dto.HoTen.Trim();
        sinhVien.NgaySinh = dto.NgaySinh;
        sinhVien.GioiTinh = dto.GioiTinh;
        sinhVien.Email = dto.Email?.Trim();
        sinhVien.SoDienThoai = dto.SoDienThoai?.Trim();
        sinhVien.DiaChi = dto.DiaChi?.Trim();
        sinhVien.LopId = dto.LopId;
        sinhVien.NienKhoa = dto.NienKhoa?.Trim();
        if (!string.IsNullOrWhiteSpace(dto.TrangThai))
        {
            sinhVien.TrangThai = dto.TrangThai.Trim();
        }

        await _db.SaveChangesAsync();

        var resultDto = new SinhVienDto
        {
            Id = sinhVien.Id,
            MaSinhVien = sinhVien.MaSinhVien,
            HoTen = sinhVien.HoTen,
            NgaySinh = sinhVien.NgaySinh.HasValue ? sinhVien.NgaySinh.Value.ToString("yyyy-MM-dd") : null,
            GioiTinh = sinhVien.GioiTinh,
            Email = sinhVien.Email,
            SoDienThoai = sinhVien.SoDienThoai,
            DiaChi = sinhVien.DiaChi,
            LopId = sinhVien.LopId,
            NienKhoa = sinhVien.NienKhoa,
            TrangThai = sinhVien.TrangThai
        };

        return Ok(ApiResponse<SinhVienDto>.Ok(resultDto, "Cập nhật sinh viên thành công"));
    }

    /// <summary>
    /// DELETE /api/sinhvien/{id}: Xóa sinh viên khỏi hệ thống (Chuẩn RESTful: HTTP 200 OK)
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var sinhVien = await _db.SinhViens.FindAsync(id);
        if (sinhVien == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Không tìm thấy sinh viên có ID = {id} để xóa"));
        }

        _db.SinhViens.Remove(sinhVien);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { deletedId = id, maSinhVien = sinhVien.MaSinhVien }, "Xóa sinh viên thành công"));
    }
}

