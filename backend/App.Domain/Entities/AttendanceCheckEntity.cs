using Base.Domain;

namespace App.Domain.Entities;

public class AttendanceCheckEntity : BaseEntity
{
    public string StudentCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string AttendanceIdentifier { get; set; } = default!;
    public AttendanceEntity? Attendance { get; set; }
    public string? WorkplaceIdentifier { get; set; }
    public WorkplaceEntity? Workplace { get; set; }
}