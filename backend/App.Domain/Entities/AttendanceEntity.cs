using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class AttendanceEntity : BaseEntity
{
    [Required]
    public string Identifier  { get; set; } = default!;
    [Required]
    public Guid CourseId { get; set; }
    public CourseEntity? Course { get; set; }
    [Required]
    public Guid ClassroomId { get; set; }
    public ClassroomEntity? Classroom { get; set; }
    [Required]
    public Guid TypeId { get; set; }
    public AttendanceTypeEntity? Type { get; set; }
    [Required]
    public bool AutomatedRegistration { get; set; } = false;
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }

    public ICollection<AttendanceCheckEntity>? AttendanceChecks { get; set; }
}