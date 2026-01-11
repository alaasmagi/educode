namespace App.DTO;

public class AttendanceStudentCountDto
{
    public Guid AttendanceId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public int StudentCount { get; set; } = 0;
}