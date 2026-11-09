namespace BTL_StudentMGMT.Models;

public class AttendanceOptions
{
    public const string SectionName = "Attendance";

    public int AbsenceThresholdPercent { get; set; } = 20;
    public int EditWindowDays { get; set; } = 7;
    public bool CountExcusedAsAbsent { get; set; } = false;
    public int MinSessionsForWarning { get; set; } = 3;
}
