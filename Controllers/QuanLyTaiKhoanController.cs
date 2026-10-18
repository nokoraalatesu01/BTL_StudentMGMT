using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_StudentMGMT.Controllers;

[Authorize(Roles = "Admin")]
public class QuanLyTaiKhoanController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuanLyTaiKhoanController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.AsNoTracking().OrderBy(user => user.UserName).ToListAsync();
        var students = await _db.SinhViens.AsNoTracking()
            .Where(student => student.UserId != null)
            .Select(student => new { student.UserId, student.MaSinhVien, student.HoTen })
            .ToListAsync();
        var teachers = await _db.GiangViens.AsNoTracking()
            .Where(teacher => teacher.UserId != null)
            .Select(teacher => new { teacher.UserId, teacher.MaGiangVien, teacher.HoTen })
            .ToListAsync();
        var model = new TaiKhoanListViewModel();
        var currentUserId = _userManager.GetUserId(User);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var student = students.FirstOrDefault(item => item.UserId == user.Id);
            var teacher = teachers.FirstOrDefault(item => item.UserId == user.Id);
            var links = new List<string>();
            if (student != null)
            {
                links.Add($"Sinh viên: {student.MaSinhVien} - {student.HoTen}");
            }
            if (teacher != null)
            {
                links.Add($"Giảng viên: {teacher.MaGiangVien} - {teacher.HoTen}");
            }

            model.TaiKhoans.Add(new TaiKhoanListItemViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                HoTen = user.HoTen,
                VaiTro = roles.Count == 0 ? "Chưa gán vai trò" : string.Join(", ", roles),
                HoSoLienKet = links.Count == 0 ? null : string.Join(" | ", links),
                IsLocked = await _userManager.IsLockedOutAsync(user),
                IsCurrentUser = user.Id == currentUserId
            });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string? loaiHoSo)
    {
        var model = new TaoTaiKhoanViewModel { LoaiHoSo = loaiHoSo ?? "" };
        await LoadProfileOptionsAsync(model);
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaoTaiKhoanViewModel model)
    {
        model.UserName = model.UserName.Trim();
        await LoadProfileOptionsAsync(model);
        if (!ModelState.IsValid)
        {
            return View("Form", model);
        }

        var profile = await FindUnlinkedProfileAsync(model.LoaiHoSo, model.HoSoId);
        if (profile == null)
        {
            ModelState.AddModelError(nameof(model.HoSoId), "Hồ sơ không tồn tại hoặc đã được liên kết với tài khoản.");
            return View("Form", model);
        }

        if (!await _db.Roles.AnyAsync(role => role.Name == model.LoaiHoSo))
        {
            ModelState.AddModelError(string.Empty, $"Vai trò {model.LoaiHoSo} chưa được cấu hình.");
            return View("Form", model);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var user = new ApplicationUser
        {
            UserName = model.UserName,
            HoTen = profile.HoTen,
            Email = profile.Email,
            PhoneNumber = profile.SoDienThoai,
            EmailConfirmed = !string.IsNullOrWhiteSpace(profile.Email),
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync();
            AddIdentityErrors(createResult);
            return View("Form", model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.LoaiHoSo);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();
            AddIdentityErrors(roleResult);
            return View("Form", model);
        }

        profile.Link(user.Id);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(nameof(model.HoSoId), "Hồ sơ vừa được liên kết với một tài khoản khác. Hãy tải lại danh sách.");
            return View("Form", model);
        }

        await transaction.CommitAsync();
        TempData["Msg"] = $"Đã tạo tài khoản {model.UserName} và liên kết hồ sơ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Profiles(string loaiHoSo)
    {
        if (loaiHoSo == "SinhVien")
        {
            var students = await _db.SinhViens.AsNoTracking()
                .Where(student => student.UserId == null)
                .OrderBy(student => student.MaSinhVien)
                .Select(student => new { student.Id, student.MaSinhVien, student.HoTen })
                .ToListAsync();
            return Json(students.Select(student => new
            {
                value = student.Id,
                text = $"{student.MaSinhVien} - {student.HoTen}"
            }));
        }

        if (loaiHoSo == "GiangVien")
        {
            var teachers = await _db.GiangViens.AsNoTracking()
                .Where(teacher => teacher.UserId == null)
                .OrderBy(teacher => teacher.MaGiangVien)
                .Select(teacher => new { teacher.Id, teacher.MaGiangVien, teacher.HoTen })
                .ToListAsync();
            return Json(teachers.Select(teacher => new
            {
                value = teacher.Id,
                text = $"{teacher.MaGiangVien} - {teacher.HoTen}"
            }));
        }

        return BadRequest("Loại hồ sơ không hợp lệ.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        if (id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "Không thể khóa chính tài khoản Admin đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        user.LockoutEnabled = true;
        var enableResult = await _userManager.UpdateAsync(user);
        if (!enableResult.Succeeded)
        {
            TempData["Error"] = $"Không thể bật khóa tài khoản: {string.Join("; ", enableResult.Errors.Select(error => error.Description))}";
            return RedirectToAction(nameof(Index));
        }

        var lockResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        if (!lockResult.Succeeded)
        {
            TempData["Error"] = $"Không thể khóa tài khoản: {string.Join("; ", lockResult.Errors.Select(error => error.Description))}";
            return RedirectToAction(nameof(Index));
        }

        TempData["Msg"] = $"Đã khóa tài khoản {user.UserName}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null);
        var resetResult = unlockResult.Succeeded
            ? await _userManager.ResetAccessFailedCountAsync(user)
            : IdentityResult.Failed(unlockResult.Errors.ToArray());
        if (!unlockResult.Succeeded || !resetResult.Succeeded)
        {
            var errors = unlockResult.Errors.Concat(resetResult.Errors).Select(error => error.Description);
            TempData["Error"] = $"Không thể mở khóa tài khoản: {string.Join("; ", errors)}";
            return RedirectToAction(nameof(Index));
        }

        TempData["Msg"] = $"Đã mở khóa tài khoản {user.UserName}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(new DatLaiMatKhauViewModel
        {
            UserId = user.Id,
            UserName = user.UserName ?? ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(DatLaiMatKhauViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        model.UserName = user.UserName ?? "";
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return View(model);
        }

        TempData["Msg"] = $"Đã đặt lại mật khẩu cho tài khoản {user.UserName}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadProfileOptionsAsync(TaoTaiKhoanViewModel model)
    {
        model.LoaiHoSoOptions =
        [
            new SelectListItem("Sinh viên", "SinhVien"),
            new SelectListItem("Giảng viên", "GiangVien")
        ];

        if (model.LoaiHoSo == "SinhVien")
        {
            model.HoSoOptions = await _db.SinhViens.AsNoTracking()
                .Where(student => student.UserId == null)
                .OrderBy(student => student.MaSinhVien)
                .Select(student => new SelectListItem(
                    student.MaSinhVien + " - " + student.HoTen,
                    student.Id.ToString()))
                .ToListAsync();
        }
        else if (model.LoaiHoSo == "GiangVien")
        {
            model.HoSoOptions = await _db.GiangViens.AsNoTracking()
                .Where(teacher => teacher.UserId == null)
                .OrderBy(teacher => teacher.MaGiangVien)
                .Select(teacher => new SelectListItem(
                    teacher.MaGiangVien + " - " + teacher.HoTen,
                    teacher.Id.ToString()))
                .ToListAsync();
        }
        else
        {
            model.HoSoOptions = new List<SelectListItem>();
        }
    }

    private async Task<UnlinkedProfile?> FindUnlinkedProfileAsync(string type, int id)
    {
        if (type == "SinhVien")
        {
            var student = await _db.SinhViens.FirstOrDefaultAsync(item => item.Id == id && item.UserId == null);
            return student == null
                ? null
                : new UnlinkedProfile(student.HoTen, student.Email, student.SoDienThoai, linkedUserId => student.UserId = linkedUserId);
        }

        if (type == "GiangVien")
        {
            var teacher = await _db.GiangViens.FirstOrDefaultAsync(item => item.Id == id && item.UserId == null);
            return teacher == null
                ? null
                : new UnlinkedProfile(teacher.HoTen, teacher.Email, teacher.SoDienThoai, linkedUserId => teacher.UserId = linkedUserId);
        }

        return null;
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 };

    private sealed record UnlinkedProfile(string HoTen, string? Email, string? SoDienThoai, Action<string> Link);
}
