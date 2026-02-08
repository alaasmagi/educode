using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

public class WorkplaceEntity : BaseEntity
{
    [Required] 
    public string Identifier { get; set; } = default!;
    [Required]
    public Guid ClassroomId { get; set; }
    public ClassroomEntity? Classroom { get; set; }
    [Required]
    [MaxLength(128)]
    public string ComputerCode { get; set; } = default!;

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (string.IsNullOrWhiteSpace(Identifier))
        {
            errors.Add(new Error("identifier-empty", "Identifier cannot be empty"));
        }

        if (ClassroomId == Guid.Empty)
        {
            errors.Add(new Error("classroom-id-empty", "ClassroomId cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(ComputerCode))
        {
            errors.Add(new Error("computer-code-empty", "ComputerCode cannot be empty"));
        }
        else if (ComputerCode.Length > 128)
        {
            errors.Add(new Error("computer-code-too-long", "ComputerCode cannot exceed 128 characters"));
        }

        return errors;
    }
}