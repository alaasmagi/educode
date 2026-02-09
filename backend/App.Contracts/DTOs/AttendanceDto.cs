using App.Domain.Entities;

namespace App.Contracts.DTOs;

public class AttendanceDto
{
    // Parameterless constructor for deserialization
    public AttendanceDto() { }
    
    public AttendanceDto(AttendanceEntity attendance)
    {
        Id = attendance.Id;
        CourseId = attendance.CourseId;
        CourseCode = attendance.Course?.Code;
        CourseName = attendance.Course?.Name;
        StudentCount = attendance.AttendanceChecks?.Count();
        AttendanceTypeId = attendance.TypeId;
        AttendanceType = attendance.Type?.TypeName;
        StartTime = attendance.StartTime;
        EndTime = attendance.EndTime;
    }

    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string? CourseCode { get; set; }
    public string? CourseName { get; set; }
    public int? StudentCount { get; set; }
    public Guid? AttendanceTypeId { get; set; }
    public string? AttendanceType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public static List<AttendanceDto> ToDtoList(List<AttendanceEntity>? entities)
    {
        return entities?.Select(e => new AttendanceDto(e)).ToList() 
               ?? new List<AttendanceDto>();
    }
}