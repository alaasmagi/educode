using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class ClassroomEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string Classroom { get; set; } = default!;
    [Required]
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public ICollection<AttendanceEntity>? Attendances { get; set; }

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(Classroom))
        {
            errors.Add(new Error("classroom-empty", "Classroom cannot be empty"));
        }
        else if (Classroom.Length > 128)
        {
            errors.Add(new Error("classroom-too-long", "Classroom cannot exceed 128 characters"));
        }

        if (SchoolId == Guid.Empty)
        {
            errors.Add(new Error("school-id-empty", "SchoolId cannot be empty"));
        }

        return errors;
    }
}