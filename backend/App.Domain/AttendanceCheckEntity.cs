using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class AttendanceCheckEntity : BaseEntity
{
    [Required]
    public string StudentCode { get; set; } = default!;
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = default!;
    [Required]
    public string AttendanceIdentifier { get; set; } = default!;
    public CourseAttendanceEntity? CourseAttendance { get; set; }
    public string? WorkplaceIdentifier { get; set; }
    public WorkplaceEntity? Workplace { get; set; }
}