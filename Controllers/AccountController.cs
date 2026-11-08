using BTL_StudentMGMT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BTL_StudentMGMT.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                return await RedirectByRole(user);
            }
        }

        var model = new LoginViewModel();

        // 1. LÀM VIỆC VỚI COOKIE (Đọc Cookie): Nếu trước đó đã tích chọn RememberMe, tự điền lại UserName
        if (Request.Cookies.TryGetValue("Remembered_UserName", out var rememberedUser) && !string.IsNullOrWhiteSpace(rememberedUser))
        {
            model.UserName = rememberedUser;
            model.RememberMe = true;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.UserName,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // 2. LÀM VIỆC VỚI COOKIE (Ghi/Xóa Cookie): Quản lý Cookie lưu tên đăng nhập
                if (model.RememberMe)
                {
                    Response.Cookies.Append("Remembered_UserName", model.UserName, new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddDays(30),
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    });
                }
                else
                {
                    Response.Cookies.Delete("Remembered_UserName");
                }

                // 3. LÀM VIỆC VỚI SESSION (Lưu Session): Lưu thông tin định danh và thời gian đăng nhập vào Session
                HttpContext.Session.SetString("Session_UserId", user.Id);
                HttpContext.Session.SetString("Session_UserName", user.UserName ?? "");
                HttpContext.Session.SetString("Session_FullName", user.HoTen ?? user.UserName ?? "");
                HttpContext.Session.SetString("Session_Role", roles.FirstOrDefault() ?? "");
                HttpContext.Session.SetString("Session_LoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                return await RedirectByRole(user);
            }
        }

        ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Xóa sạch toàn bộ Session khi người dùng đăng xuất
        HttpContext.Session.Clear();

        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<IActionResult> RedirectByRole(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Admin"))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (roles.Contains("GiangVien"))
        {
            return RedirectToAction("Index", "GiangVien");
        }

        if (roles.Contains("PGV"))
        {
            return RedirectToAction("Index", "Admin");
        }

        return RedirectToAction("Index", "SinhVien");
    }
}
