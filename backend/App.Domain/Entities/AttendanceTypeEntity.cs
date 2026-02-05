using Base.Domain;

namespace App.Domain.Entities;

public class AttendanceTypeEntity : BaseEntity
{
    public string TypeName { get; set; } = default!;
}