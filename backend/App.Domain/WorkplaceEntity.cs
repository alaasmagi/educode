using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

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
}