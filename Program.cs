using BTL_StudentMGMT.Data;
using BTL_StudentMGMT.Models;
using BTL_StudentMGMT.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = DbConnectionHelper.ResolveConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseSqlServer(connectionString);
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();

// Cấu hình Quốc tế hóa & Đa ngôn ngữ (Localization / i18n)
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "vi-VN", "en-US" };
    options.SetDefaultCulture("vi-VN")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Cấu hình Distributed Memory Cache & Session (Phục vụ lưu trữ dữ liệu phiên làm việc)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".UTC.StudentPortal.Session";
});

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IGiangVienSecurityService, GiangVienSecurityService>();
builder.Services.AddScoped<IVietQrService, VietQrService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();

var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Lỗi xảy ra trong quá trình Seed database ban đầu nhưng ứng dụng vẫn tiếp tục chạy.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Thiết lập các HTTP Security Headers chuẩn OWASP (Bảo vệ chống Clickjacking, MIME sniffing, XSS)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseRouting();

// Kích hoạt Middleware đa ngôn ngữ (Localization)
app.UseRequestLocalization();

// Kích hoạt Session Middleware (đặt sau UseRouting và trước UseAuthentication/UseAuthorization)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
// hot reload touch
