using App.Domain.Entities;

namespace App.Application.DTOs;

public class CourseAttendanceDto(AttendanceEntity attendance)
{
    public Guid Id { get; set; } = attendance.Id;
    public Guid CourseId { get; set; } = attendance.CourseId;
    public string? CourseCode { get; set; } = attendance.Course?.Code;
    public string? CourseName { get; set; } = attendance.Course?.Name;
    public int? StudentCount { get; set; } = attendance.AttendanceChecks?.Count();
    public Guid? AttendanceTypeId { get; set; } = attendance.TypeId;
    public string? AttendanceType { get; set; } = attendance.Type?.TypeName;
    public DateTime StartTime { get; set; } = attendance.StartTime;
    public DateTime EndTime { get; set; } = attendance.EndTime;
    
    
    public static List<CourseAttendanceDto> ToDtoList(List<AttendanceEntity>? entities)
    {
        if (entities == null)
        {
            return new List<CourseAttendanceDto>();
        }
        return entities.Select(e => new CourseAttendanceDto(e)).ToList();
    }
}