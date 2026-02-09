namespace App.Contracts.DTOs;

public class AttendanceStudentCountDto
{
    // Parameterless constructor for deserialization
    public AttendanceStudentCountDto() { }
    
    public AttendanceStudentCountDto(Guid attendanceId, DateTime attendanceDate, int studentCount)
    {
        AttendanceId = attendanceId;
        AttendanceDate = attendanceDate;
        StudentCount = studentCount;
    }

    public Guid AttendanceId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public int StudentCount { get; set; }
    
    public static List<AttendanceStudentCountDto> AttendanceStudentCountDtos(List<(Guid, DateTime, int)> data)
    {
        return data.Select(d => new AttendanceStudentCountDto(d.Item1, d.Item2, d.Item3)).ToList();
    }
}
