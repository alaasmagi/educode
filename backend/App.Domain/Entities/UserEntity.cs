using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class UserEntity : BaseEntity
{
    [Required]
    public Guid TypeId { get; set; }
    public UserTypeEntity? Type { get; set; }
    public Guid? SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    [Required]
    [MaxLength(128)]
    public string Email { get; set; } = default!;
    [MaxLength(128)]
    public string? StudentCode { get; set; }
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = default!;
    [MaxLength(255)]
    public string? PhotoPath { get; set; } = default!;
    public List<RefreshTokenEntity>? RefreshTokens { get; set; } = default!;

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (TypeId == Guid.Empty)
        {
            errors.Add(new Error("type-id-empty", "TypeId cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            errors.Add(new Error("email-empty", "Email cannot be empty"));
        }
        else if (Email.Length > 128)
        {
            errors.Add(new Error("email-too-long", "Email cannot exceed 128 characters"));
        }
        else if (!IsValidEmail(Email))
        {
            errors.Add(new Error("email-invalid", "Email format is invalid"));
        }

        if (!string.IsNullOrWhiteSpace(StudentCode) && StudentCode.Length > 128)
        {
            errors.Add(new Error("student-code-too-long", "StudentCode cannot exceed 128 characters"));
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            errors.Add(new Error("full-name-empty", "FullName cannot be empty"));
        }
        else if (FullName.Length > 255)
        {
            errors.Add(new Error("full-name-too-long", "FullName cannot exceed 255 characters"));
        }

        if (!string.IsNullOrWhiteSpace(PhotoPath) && PhotoPath.Length > 255)
        {
            errors.Add(new Error("photo-path-too-long", "PhotoPath cannot exceed 255 characters"));
        }

        return errors;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}