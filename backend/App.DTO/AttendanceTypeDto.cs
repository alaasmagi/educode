using App.Domain;

namespace App.DTO;

public class AttendanceTypeDto(AttendanceTypeEntity attendanceType)
{
    public Guid Id { get; set; } = attendanceType.Id;
    public string AttendanceType { get; set; } = attendanceType.AttendanceType;
    
    public static List<AttendanceTypeDto> ToDtoList(List<AttendanceTypeEntity>? entities)
    {
        if (entities == null)
        {
            return new List<AttendanceTypeDto>();
        }
        return entities.Select(e => new AttendanceTypeDto(e)).ToList();
    }
}