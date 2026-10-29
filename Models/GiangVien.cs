using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class GiangVien
{
    public int Id { get; set; }

    [Required]
    public string MaGiangVien { get; set; } = "";

    [Required]
    public string HoTen { get; set; } = "";

    public string? Email { get; set; }
    public string? SoDienThoai { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public int? DonViId { get; set; }
    public virtual DonVi? DonVi { get; set; }
}
