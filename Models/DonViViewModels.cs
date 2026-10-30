using System.ComponentModel.DataAnnotations;

namespace BTL_StudentMGMT.Models;

public class DonViListItemViewModel
{
    public int DonViId { get; set; }
    public string MaDonVi { get; set; } = "";
    public string TenDonVi { get; set; } = "";
    public string LoaiDonVi { get; set; } = "";
    public string? TenDonViCha { get; set; }
    public string? Email { get; set; }
    public string? DienThoai { get; set; }
    public string TrangThai { get; set; } = "ACTIVE";
    public int SoGiangVien { get; set; }
    public int SoMonHoc { get; set; }
    public int SoNganh { get; set; }
    public int SoDonViCon { get; set; }
}

public class DonViFormViewModel
{
    public int DonViId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã đơn vị")]
    [StringLength(20, ErrorMessage = "Mã đơn vị không được vượt quá 20 ký tự")]
    [Display(Name = "Mã đơn vị")]
    public string MaDonVi { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập tên đơn vị")]
    [StringLength(200, ErrorMessage = "Tên đơn vị không được vượt quá 200 ký tự")]
    [Display(Name = "Tên đơn vị")]
    public string TenDonVi { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn loại đơn vị")]
    [StringLength(20, ErrorMessage = "Loại đơn vị không được vượt quá 20 ký tự")]
    [Display(Name = "Loại đơn vị")]
    public string LoaiDonVi { get; set; } = "KHOA";

    [Display(Name = "Đơn vị trực thuộc (Đơn vị cha)")]
    public int? DonViChaId { get; set; }

    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    [StringLength(254, ErrorMessage = "Email không vượt quá 254 ký tự")]
    [Display(Name = "Email liên hệ")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Điện thoại không vượt quá 20 ký tự")]
    [Display(Name = "Số điện thoại")]
    public string? DienThoai { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
    [StringLength(20, ErrorMessage = "Trạng thái không vượt quá 20 ký tự")]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "ACTIVE";
}
