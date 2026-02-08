using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class CourseStatusEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string StatusName { get; set; } = default!;

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(StatusName))
        {
            errors.Add(new Error("status-name-empty", "StatusName cannot be empty"));
        }
        else if (StatusName.Length > 128)
        {
            errors.Add(new Error("status-name-too-long", "StatusName cannot exceed 128 characters"));
        }

        return errors;
    }
}