using Base.Domain;

namespace App.Domain.Entities;

public class SchoolEntity : BaseEntity
{
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    public string Domain { get; set; } = default!;
    public string StudentCodePattern { get; set; } = default!;
    
    public ICollection<ClassroomEntity>? Classrooms { get; set; }
}