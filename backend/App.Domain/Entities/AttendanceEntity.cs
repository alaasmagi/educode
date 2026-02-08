using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class AttendanceEntity : BaseEntity
{
    [Required]
    public string Identifier  { get; set; } = default!;
    [Required]
    public Guid CourseId { get; set; }
    public CourseEntity? Course { get; set; }
    [Required]
    public Guid ClassroomId { get; set; }
    public ClassroomEntity? Classroom { get; set; }
    [Required]
    public Guid TypeId { get; set; }
    public AttendanceTypeEntity? Type { get; set; }
    [Required]
    public bool AutomatedRegistration { get; set; } = false;
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }

    public ICollection<AttendanceCheckEntity>? AttendanceChecks { get; set; }

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(Identifier))
        {
            errors.Add(new Error("identifier-empty", "Identifier cannot be empty"));
        }

        if (CourseId == Guid.Empty)
        {
            errors.Add(new Error("course-id-empty", "CourseId cannot be empty"));
        }

        if (ClassroomId == Guid.Empty)
        {
            errors.Add(new Error("classroom-id-empty", "ClassroomId cannot be empty"));
        }

        if (TypeId == Guid.Empty)
        {
            errors.Add(new Error("type-id-empty", "TypeId cannot be empty"));
        }

        if (StartTime == default)
        {
            errors.Add(new Error("start-time-empty", "StartTime cannot be empty"));
        }

        if (EndTime == default)
        {
            errors.Add(new Error("end-time-empty", "EndTime cannot be empty"));
        }

        if (StartTime >= EndTime)
        {
            errors.Add(new Error("invalid-time-range", "StartTime must be before EndTime"));
        }

        return errors;
    }
}