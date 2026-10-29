using System;
using System.Collections.Generic;

namespace BTL_StudentMGMT.TempTestBieuPhi;

public partial class BieuPhiHocPhi
{
    public int BieuPhiId { get; set; }

    public int? ChuongTrinhId { get; set; }

    public int NamHocId { get; set; }

    public decimal DonGiaTinChi { get; set; }

    public string LoaiTien { get; set; } = null!;

    public DateOnly HieuLucTu { get; set; }

    public DateOnly? HieuLucDen { get; set; }
}
