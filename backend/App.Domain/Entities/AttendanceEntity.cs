using Base.Domain;

namespace App.Domain.Entities;

public class AttendanceEntity : BaseEntity
{
    public string Identifier  { get; set; } = default!;
    public Guid CourseId { get; set; }
    public CourseEntity? Course { get; set; }
    public Guid ClassroomId { get; set; }
    public ClassroomEntity? Classroom { get; set; }
    public Guid TypeId { get; set; }
    public AttendanceTypeEntity? Type { get; set; }
    public bool AutomatedRegistration { get; set; } = false;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public ICollection<AttendanceCheckEntity>? AttendanceChecks { get; set; }
}