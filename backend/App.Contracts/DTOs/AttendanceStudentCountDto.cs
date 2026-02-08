namespace App.Contracts.DTOs;

public class AttendanceStudentCountDto(Guid attendanceId, DateTime attendanceDate, int studentCount)
{
    public Guid AttendanceId { get; set; } = attendanceId;
    public DateTime AttendanceDate { get; set; } = attendanceDate;
    public int StudentCount { get; set; } = studentCount;
    
    public static List<AttendanceStudentCountDto> AttendanceStudentCountDtos(List<(Guid, DateTime, int)> data)
    {
        return data.Select(d => new AttendanceStudentCountDto(d.Item1, d.Item2, d.Item3)).ToList();
    }
}
