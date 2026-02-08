using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class CourseEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string Code { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = default!;
    [Required]
    public bool CrossUniRegistration { get; set; } = false;
    [Required]
    public Guid StatusId { get; set; }
    public CourseStatusEntity? Status { get; set; }
    [Required]
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public ICollection<CourseTeacherEntity>? Teachers { get; set; }

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(Code))
        {
            errors.Add(new Error("code-empty", "Code cannot be empty"));
        }
        else if (Code.Length > 128)
        {
            errors.Add(new Error("code-too-long", "Code cannot exceed 128 characters"));
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add(new Error("name-empty", "Name cannot be empty"));
        }
        else if (Name.Length > 128)
        {
            errors.Add(new Error("name-too-long", "Name cannot exceed 128 characters"));
        }

        if (StatusId == Guid.Empty)
        {
            errors.Add(new Error("status-id-empty", "StatusId cannot be empty"));
        }

        if (SchoolId == Guid.Empty)
        {
            errors.Add(new Error("school-id-empty", "SchoolId cannot be empty"));
        }

        return errors;
    }
}
