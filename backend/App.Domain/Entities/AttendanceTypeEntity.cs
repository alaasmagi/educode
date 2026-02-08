using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class AttendanceTypeEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string TypeName { get; set; } = default!;

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(TypeName))
        {
            errors.Add(new Error("type-name-empty", "TypeName cannot be empty"));
        }
        else if (TypeName.Length > 128)
        {
            errors.Add(new Error("type-name-too-long", "TypeName cannot exceed 128 characters"));
        }

        return errors;
    }
}