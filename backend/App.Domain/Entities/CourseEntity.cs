using Base.Domain;

namespace App.Domain.Entities;

public class CourseEntity : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool CrossUniRegistration { get; set; } = false;
    public Guid StatusId { get; set; }
    public CourseStatusEntity? Status { get; set; }
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public ICollection<CourseTeacherEntity>? Teachers { get; set; }
}