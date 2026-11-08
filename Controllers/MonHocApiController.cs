using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

/// <summary>
/// RESTful Web API Controller quản lý Môn học
/// Đường dẫn: /api/monhoc
/// </summary>
[ApiController]
[Route("api/monhoc")]
[Produces("application/json")]
public class MonHocApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MonHocApiController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /api/monhoc: Lấy danh sách toàn bộ môn học
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword)
    {
        var query = _db.MonHocs
            .AsNoTracking()
            .Include(m => m.DonVi)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(m => m.MaMon.ToLower().Contains(kw) || m.TenMon.ToLower().Contains(kw));
        }

        var list = await query
            .OrderBy(m => m.MaMon)
            .Select(m => new MonHocDto
            {
                Id = m.Id,
                MaMon = m.MaMon,
                TenMon = m.TenMon,
                SoTinChi = m.SoTinChi,
                DonViId = m.DonViId,
                TenDonVi = m.DonVi != null ? m.DonVi.TenDonVi : null
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MonHocDto>>.Ok(list, $"Lấy danh sách thành công ({list.Count} môn học)"));
    }

    /// <summary>
    /// GET /api/monhoc/{id}: Lấy chi tiết môn học theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _db.MonHocs
            .AsNoTracking()
            .Include(x => x.DonVi)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (m == null)
        {
            return NotFound(ApiResponse<object>.Fail($"Không tìm thấy môn học có ID = {id}"));
        }

        var dto = new MonHocDto
        {
            Id = m.Id,
            MaMon = m.MaMon,
            TenMon = m.TenMon,
            SoTinChi = m.SoTinChi,
            DonViId = m.DonViId,
            TenDonVi = m.DonVi?.TenDonVi
        };

        return Ok(ApiResponse<MonHocDto>.Ok(dto, "Lấy thông tin môn học thành công"));
    }
}

