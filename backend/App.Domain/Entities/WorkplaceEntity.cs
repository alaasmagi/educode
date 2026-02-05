using Base.Domain;

namespace App.Domain.Entities;

public class WorkplaceEntity : BaseEntity
{
    public string Identifier { get; set; } = default!;
    public Guid ClassroomId { get; set; }
    public ClassroomEntity? Classroom { get; set; }
    public string ComputerCode { get; set; } = default!;
}