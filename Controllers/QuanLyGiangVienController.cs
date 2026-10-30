using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLyGiangVienController : Controller
{
    private const int PageSize = 10;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileUploadService _fileUploadService;

    public QuanLyGiangVienController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IFileUploadService fileUploadService)
    {
        _db = db;
        _userManager = userManager;
        _fileUploadService = fileUploadService;
    }

    public async Task<IActionResult> Index(string? tuKhoa, int page = 1)
    {
        page = Math.Max(1, page);
        var query = _db.GiangViens
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.DonVi)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var keyword = tuKhoa.Trim();
            query = query.Where(item => item.MaGiangVien.Contains(keyword) || item.HoTen.Contains(keyword));
        }

        var totalItems = await query.CountAsync();
        page = Math.Min(page, Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize)));
        var model = new PagedResult<GiangVien>
        {
            Items = await query.OrderBy(item => item.MaGiangVien)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync(),
            Page = page,
            PageSize = PageSize,
            TotalItems = totalItems
        };

        ViewBag.TuKhoa = tuKhoa;
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var giangVien = await _db.GiangViens
            .AsNoTracking()
            .Include(item => item.User)
            .Include(item => item.DonVi)
            .FirstOrDefaultAsync(item => item.Id == id);
        return giangVien == null ? NotFound() : View(giangVien);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDonViDropdownAsync();
        return View("Form", new GiangVienFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GiangVienFormViewModel model)
    {
        Normalize(model);
        await ValidateCodeAsync(model.MaGiangVien);

        if (!ModelState.IsValid)
        {
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        string? avatarPath = null;
        if (model.FileAnh != null && model.FileAnh.Length > 0)
        {
            avatarPath = await _fileUploadService.UploadAvatarAsync(model.FileAnh);
        }

        _db.GiangViens.Add(new GiangVien
        {
            MaGiangVien = model.MaGiangVien,
            HoTen = model.HoTen,
            Email = model.Email,
            SoDienThoai = model.SoDienThoai,
            DonViId = model.DonViId,
            HinhAnh = avatarPath
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaGiangVien), "Mã giảng viên đã tồn tại.");
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        TempData["Msg"] = "Đã thêm giảng viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var giangVien = await _db.GiangViens.FindAsync(id);
        if (giangVien == null)
        {
            return NotFound();
        }

        await LoadDonViDropdownAsync(giangVien.DonViId);
        return View("Form", new GiangVienFormViewModel
        {
            Id = giangVien.Id,
            MaGiangVien = giangVien.MaGiangVien,
            HoTen = giangVien.HoTen,
            Email = giangVien.Email,
            SoDienThoai = giangVien.SoDienThoai,
            DonViId = giangVien.DonViId,
            HinhAnh = giangVien.HinhAnh
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GiangVienFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var giangVien = await _db.GiangViens.FindAsync(id);
        if (giangVien == null)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateCodeAsync(model.MaGiangVien, id);
        if (!ModelState.IsValid)
        {
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        if (model.FileAnh != null && model.FileAnh.Length > 0)
        {
            var avatarPath = await _fileUploadService.UploadAvatarAsync(model.FileAnh, giangVien.HinhAnh);
            if (!string.IsNullOrEmpty(avatarPath))
            {
                giangVien.HinhAnh = avatarPath;
            }
        }

        giangVien.MaGiangVien = model.MaGiangVien;
        giangVien.HoTen = model.HoTen;
        giangVien.Email = model.Email;
        giangVien.SoDienThoai = model.SoDienThoai;
        giangVien.DonViId = model.DonViId;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            ModelState.AddModelError(nameof(model.MaGiangVien), "Mã giảng viên đã tồn tại.");
            await LoadDonViDropdownAsync(model.DonViId);
            return View("Form", model);
        }

        TempData["Msg"] = "Đã cập nhật thông tin giảng viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var giangVien = await _db.GiangViens.FindAsync(id);
        if (giangVien == null)
        {
            return NotFound();
        }

        _db.GiangViens.Remove(giangVien);
        _fileUploadService.DeleteFile(giangVien.HinhAnh);
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Đã xóa giảng viên.";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(GiangVienFormViewModel model)
    {
        model.MaGiangVien = (model.MaGiangVien ?? "").Trim();
        model.HoTen = (model.HoTen ?? "").Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();
    }

    private async Task ValidateCodeAsync(string maGiangVien, int? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(maGiangVien))
        {
            return;
        }

        var query = _db.GiangViens.AsNoTracking().Where(item => item.MaGiangVien == maGiangVien);
        if (currentId.HasValue)
        {
            query = query.Where(item => item.Id != currentId.Value);
        }

        if (await query.AnyAsync())
        {
            ModelState.AddModelError(nameof(GiangVienFormViewModel.MaGiangVien), "Mã giảng viên đã tồn tại.");
        }
    }

    private async Task LoadDonViDropdownAsync(int? selectedId = null)
    {
        var units = await _db.DonVis.AsNoTracking().Where(d => d.TrangThai == "ACTIVE").OrderBy(d => d.TenDonVi).ToListAsync();
        ViewBag.DonViList = new SelectList(units, nameof(DonVi.DonViId), nameof(DonVi.TenDonVi), selectedId);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627);
    }
}
