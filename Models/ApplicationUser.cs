using Microsoft.AspNetCore.Identity;

namespace BTL_StudentMGMT.Models;

public class ApplicationUser : IdentityUser
{
    public string? HoTen { get; set; }
}
