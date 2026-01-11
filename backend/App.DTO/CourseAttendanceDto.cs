using App.Domain;

namespace App.DTO;

public class CourseAttendanceDto(CourseAttendanceEntity courseAttendance)
{
    public Guid Id { get; set; } = courseAttendance.Id;
    public Guid CourseId { get; set; } = courseAttendance.CourseId;
    public string? CourseCode { get; set; } = courseAttendance.Course?.CourseCode;
    public string? CourseName { get; set; } = courseAttendance.Course?.CourseName;
    public int? StudentCount { get; set; } = courseAttendance.AttendanceChecks?.Count();
    public Guid? AttendanceTypeId { get; set; } = courseAttendance.AttendanceTypeId;
    public string? AttendanceType { get; set; } = courseAttendance.AttendanceType?.AttendanceType;
    public DateTime StartTime { get; set; } = courseAttendance.StartTime;
    public DateTime EndTime { get; set; } = courseAttendance.EndTime;
    
    
    public static List<CourseAttendanceDto> ToDtoList(List<CourseAttendanceEntity>? entities)
    {
        if (entities == null)
        {
            return new List<CourseAttendanceDto>();
        }
        return entities.Select(e => new CourseAttendanceDto(e)).ToList();
    }
    
}