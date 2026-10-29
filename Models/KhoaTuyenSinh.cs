using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.Models;

public partial class KhoaTuyenSinh
{
    public int KhoaTuyenSinhId { get; set; }

    public string MaKhoa { get; set; } = null!;

    public string TenKhoa { get; set; } = null!;

    public short NamTuyenSinh { get; set; }
}
