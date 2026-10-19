using BTL_StudentMGMT.Models;

namespace BTL_StudentMGMT.Data;

public static class NganhCatalog
{
    public static IReadOnlyList<string> Faculties { get; } =
    [
        "Công trình và Môi trường",
        "Cơ khí",
        "Điện - Điện tử",
        "Công nghệ thông tin",
        "Kinh tế - Vận tải",
        "Khoa học cơ bản",
        "Lý luận chính trị"
    ];

    public static IReadOnlyList<Nganh> Programs { get; } =
    [
        new() { MaNganh = "7580205", TenNganh = "Kỹ thuật xây dựng công trình giao thông", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7580202", TenNganh = "Kỹ thuật xây dựng công trình thủy", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7580210", TenNganh = "Kỹ thuật cơ sở hạ tầng", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7580106", TenNganh = "Quản lý đô thị và công trình", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520103", TenNganh = "Kỹ thuật cơ khí", TenKhoa = "Cơ khí", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520115", TenNganh = "Kỹ thuật nhiệt", TenKhoa = "Cơ khí", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520114", TenNganh = "Kỹ thuật cơ điện tử", TenKhoa = "Cơ khí", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520116", TenNganh = "Kỹ thuật cơ khí động lực", TenKhoa = "Cơ khí", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520130", TenNganh = "Kỹ thuật ô tô", TenKhoa = "Cơ khí", TichHopCuNhanKySu = true },
        new() { MaNganh = "7340301", TenNganh = "Kế toán", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7310101", TenNganh = "Kinh tế", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7840104", TenNganh = "Kinh tế vận tải", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7340101", TenNganh = "Quản trị kinh doanh", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7840101", TenNganh = "Khai thác vận tải", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7810103", TenNganh = "Quản trị dịch vụ du lịch và lữ hành", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7510605", TenNganh = "Logistics và quản lý chuỗi cung ứng", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7340201", TenNganh = "Tài chính - Ngân hàng", TenKhoa = "Kinh tế - Vận tải" },
        new() { MaNganh = "7580301", TenNganh = "Kinh tế xây dựng", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7580302", TenNganh = "Quản lý xây dựng", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520201", TenNganh = "Kỹ thuật điện", TenKhoa = "Điện - Điện tử", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520207", TenNganh = "Điện tử - viễn thông", TenKhoa = "Điện - Điện tử", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520216", TenNganh = "Điều khiển và tự động hóa", TenKhoa = "Điện - Điện tử", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520218", TenNganh = "Robot và trí tuệ nhân tạo", TenKhoa = "Điện - Điện tử", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520219", TenNganh = "Hệ thống giao thông thông minh", TenKhoa = "Điện - Điện tử", TichHopCuNhanKySu = true },
        new() { MaNganh = "7480106", TenNganh = "Kỹ thuật máy tính", TenKhoa = "Điện - Điện tử", TichHopCuNhanKySu = true },
        new() { MaNganh = "7480201", TenNganh = "Công nghệ thông tin", TenKhoa = "Công nghệ thông tin", TichHopCuNhanKySu = true },
        new() { MaNganh = "7480101", TenNganh = "Khoa học máy tính", TenKhoa = "Công nghệ thông tin", TichHopCuNhanKySu = true },
        new() { MaNganh = "7580201", TenNganh = "Kỹ thuật xây dựng", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7580101", TenNganh = "Kiến trúc", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7510104", TenNganh = "Công nghệ kỹ thuật giao thông", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7520320", TenNganh = "Kỹ thuật môi trường", TenKhoa = "Công trình và Môi trường", TichHopCuNhanKySu = true },
        new() { MaNganh = "7460112", TenNganh = "Toán ứng dụng", TenKhoa = "Khoa học cơ bản" },
        new() { MaNganh = "7220201", TenNganh = "Ngôn ngữ Anh", TenKhoa = "Khoa học cơ bản" },
        new() { MaNganh = "7340120", TenNganh = "Kinh doanh quốc tế", TenKhoa = "Kinh tế - Vận tải" }
    ];
}
