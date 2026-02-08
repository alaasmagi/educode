using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class AttendanceCheckEntity : BaseEntity
{
    [Required]
    public string StudentCode { get; set; } = default!;
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = default!;
    [Required]
    public string AttendanceIdentifier { get; set; } = default!;
    public AttendanceEntity? Attendance { get; set; }
    public string? WorkplaceIdentifier { get; set; }
    public WorkplaceEntity? Workplace { get; set; }

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(StudentCode))
        {
            errors.Add(new Error("student-code-empty", "StudentCode cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            errors.Add(new Error("full-name-empty", "FullName cannot be empty"));
        }
        else if (FullName.Length > 255)
        {
            errors.Add(new Error("full-name-too-long", "FullName cannot exceed 255 characters"));
        }

        if (string.IsNullOrWhiteSpace(AttendanceIdentifier))
        {
            errors.Add(new Error("attendance-identifier-empty", "AttendanceIdentifier cannot be empty"));
        }

        return errors;
    }
}