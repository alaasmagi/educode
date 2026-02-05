using Base.Domain;

namespace App.Domain.Entities;

public class CourseTeacherEntity : BaseEntity
{
    public Guid CourseId { get; set; }
    public CourseEntity? Course { get; set; }
    public Guid TeacherId { get; set; }
    public UserEntity? Teacher { get; set; }
}