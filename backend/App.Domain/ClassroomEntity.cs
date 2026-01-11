using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class ClassroomEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string Classroom { get; set; } = default!;
    [Required]
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public ICollection<CourseAttendanceEntity>? CourseAttendances { get; set; }
}