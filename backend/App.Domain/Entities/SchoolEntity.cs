using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class SchoolEntity : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string ShortName { get; set; } = default!;
    [Required]
    [MaxLength(255)]
    public string Domain { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string StudentCodePattern { get; set; } = default!;
    
    public ICollection<ClassroomEntity>? Classrooms { get; set; }

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(Name))
        {
            errors.Add(new Error("name-empty", "Name cannot be empty"));
        }
        else if (Name.Length > 255)
        {
            errors.Add(new Error("name-too-long", "Name cannot exceed 255 characters"));
        }

        if (string.IsNullOrWhiteSpace(ShortName))
        {
            errors.Add(new Error("short-name-empty", "ShortName cannot be empty"));
        }
        else if (ShortName.Length > 128)
        {
            errors.Add(new Error("short-name-too-long", "ShortName cannot exceed 128 characters"));
        }

        if (string.IsNullOrWhiteSpace(Domain))
        {
            errors.Add(new Error("domain-empty", "Domain cannot be empty"));
        }
        else if (Domain.Length > 255)
        {
            errors.Add(new Error("domain-too-long", "Domain cannot exceed 255 characters"));
        }

        if (string.IsNullOrWhiteSpace(StudentCodePattern))
        {
            errors.Add(new Error("student-code-pattern-empty", "StudentCodePattern cannot be empty"));
        }
        else if (StudentCodePattern.Length > 128)
        {
            errors.Add(new Error("student-code-pattern-too-long", "StudentCodePattern cannot exceed 128 characters"));
        }

        return errors;
    }
}
