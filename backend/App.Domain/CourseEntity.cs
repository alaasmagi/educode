using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class CourseEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string Code { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = default!;
    [Required]
    public bool CrossUniRegistration { get; set; } = false;
    [Required]
    public Guid StatusId { get; set; }
    public CourseStatusEntity? Status { get; set; }
    [Required]
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public ICollection<CourseTeacherEntity>? Teachers { get; set; }
}