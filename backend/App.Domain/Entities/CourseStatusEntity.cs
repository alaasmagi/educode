using Base.Domain;

namespace App.Domain.Entities;

public class CourseStatusEntity : BaseEntity
{
    public string StatusName { get; set; } = default!;
}