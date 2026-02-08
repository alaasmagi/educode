using System.ComponentModel.DataAnnotations;
using App.Domain.Enums;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class UserTypeEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string TypeName { get; set; } = default!;
    [Required] 
    public EAccessLevel AccessLevel { get; set; } = EAccessLevel.NoAccess;

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

        if (!Enum.IsDefined(typeof(EAccessLevel), AccessLevel))
        {
            errors.Add(new Error("access-level-invalid", "AccessLevel is not a valid value"));
        }

        return errors;
    }
}