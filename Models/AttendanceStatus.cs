namespace BTL_StudentMGMT.Models;

public static class AttendanceStatus
{
    public const string Present = "PRESENT";
    public const string Absent = "ABSENT";
    public const string Excused = "EXCUSED";
    public const string Late = "LATE";

    public static readonly string[] All = { Present, Absent, Excused, Late };

    public static bool IsValid(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;
        var s = Normalize(status);
        return s is Present or Absent or Excused or Late;
    }

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var trimmed = raw.Trim();
        return trimmed switch
        {
            "Có mặt" or "Co mat" or "CoMat" or "PRESENT" or "present" => Present,
            "Vắng" or "Vang" or "ABSENT" or "absent" => Absent,
            "Có phép" or "Co phep" or "CoPhep" or "EXCUSED" or "excused" => Excused,
            "Đi muộn" or "Di muon" or "DiMuon" or "LATE" or "late" => Late,
            _ => trimmed.ToUpperInvariant()
        };
    }

    public static string ToDisplayName(string? status)
    {
        return Normalize(status) switch
        {
            Present => "Có mặt",
            Absent => "Vắng",
            Excused => "Có phép",
            Late => "Đi muộn",
            _ => string.IsNullOrWhiteSpace(status) ? "Chưa điểm danh" : status
        };
    }

    public static string GetBadgeClass(string? status)
    {
        return Normalize(status) switch
        {
            Present => "bg-success text-white",
            Absent => "bg-danger text-white",
            Excused => "bg-warning text-dark",
            Late => "bg-info text-dark",
            _ => "bg-secondary text-white"
        };
    }

    public static string ToBadgeClass(string? status) => GetBadgeClass(status);
}
