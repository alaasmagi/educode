using Base.Domain;

namespace App.Domain.Entities;

public class ClassroomEntity : BaseEntity
{
    public string Classroom { get; set; } = default!;
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public ICollection<AttendanceEntity>? Attendances { get; set; }
}