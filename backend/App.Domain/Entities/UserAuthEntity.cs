using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class UserAuthEntity : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = default!;
    [Required] 
    public bool Verified { get; set; } = false;

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (UserId == Guid.Empty)
        {
            errors.Add(new Error("user-id-empty", "UserId cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(PasswordHash))
        {
            errors.Add(new Error("password-hash-empty", "PasswordHash cannot be empty"));
        }
        else if (PasswordHash.Length > 255)
        {
            errors.Add(new Error("password-hash-too-long", "PasswordHash cannot exceed 255 characters"));
        }

        return errors;
    }
}