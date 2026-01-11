using App.Domain;

namespace App.DTO;

public class AttendanceCheckDto(AttendanceCheckEntity attendanceCheck)
{
    public Guid Id { get; set; } = attendanceCheck.Id;
    public string StudentCode { get; set; } = attendanceCheck.StudentCode;
    public string FullName { get; set; } = attendanceCheck.FullName;
    public string AttendanceIdentifier { get; set; } = attendanceCheck.AttendanceIdentifier;
    public string? WorkplaceIdentifier { get; set; } = attendanceCheck.WorkplaceIdentifier;
    
    public static List<AttendanceCheckDto> ToDtoList(List<AttendanceCheckEntity>? entities)
    {
        if (entities == null)
        {
            return new List<AttendanceCheckDto>();
        }
        return entities.Select(e => new AttendanceCheckDto(e)).ToList();
    }
}

