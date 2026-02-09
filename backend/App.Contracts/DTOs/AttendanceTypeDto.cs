using App.Domain.Entities;

namespace App.Contracts.DTOs;

public class AttendanceTypeDto
{
    // Parameterless constructor for deserialization
    public AttendanceTypeDto() { }
    
    public AttendanceTypeDto(AttendanceTypeEntity attendanceType)
    {
        Id = attendanceType.Id;
        AttendanceType = attendanceType.TypeName;
    }

    public Guid Id { get; set; }
    public string AttendanceType { get; set; } = string.Empty;
    
    public static List<AttendanceTypeDto> ToDtoList(List<AttendanceTypeEntity>? entities)
    {
        return entities?.Select(e => new AttendanceTypeDto(e)).ToList() 
               ?? new List<AttendanceTypeDto>();
    }
}