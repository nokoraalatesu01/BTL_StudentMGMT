using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tài khoản")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
