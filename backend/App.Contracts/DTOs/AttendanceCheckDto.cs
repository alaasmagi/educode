using App.Domain.Entities;

namespace App.Contracts.DTOs;

public class AttendanceCheckDto
{
    // Parameterless constructor for deserialization
    public AttendanceCheckDto() { }
    
    public AttendanceCheckDto(AttendanceCheckEntity attendanceCheck)
    {
        Id = attendanceCheck.Id;
        StudentCode = attendanceCheck.StudentCode;
        FullName = attendanceCheck.FullName;
        AttendanceIdentifier = attendanceCheck.AttendanceIdentifier;
        WorkplaceIdentifier = attendanceCheck.WorkplaceIdentifier;
    }

    public Guid Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AttendanceIdentifier { get; set; } = string.Empty;
    public string? WorkplaceIdentifier { get; set; }
    
    public static List<AttendanceCheckDto> ToDtoList(List<AttendanceCheckEntity>? entities)
    {
        return entities?.Select(e => new AttendanceCheckDto(e)).ToList() 
               ?? new List<AttendanceCheckDto>();
    }
}

