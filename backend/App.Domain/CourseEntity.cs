using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class CourseEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string CourseCode { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string CourseName { get; set; } = default!;
    [Required]
    public bool CrossUniRegistration { get; set; } = false;
    [Required]
    public Guid CourseStatusId { get; set; }
    public CourseStatusEntity? CourseStatus { get; set; }
    [Required]
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public ICollection<CourseTeacherEntity>? CourseTeacherEntities { get; set; }
}