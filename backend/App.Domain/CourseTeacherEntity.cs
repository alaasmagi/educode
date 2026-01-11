using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Domain;

namespace App.Domain;

public class CourseTeacherEntity : BaseEntity
{
    [Required]
    [ForeignKey("Course")]
    public Guid CourseId { get; set; }
    public CourseEntity? Course { get; set; }
    [Required]
    [ForeignKey("Teacher")]
    public Guid TeacherId { get; set; }
    public UserEntity? Teacher { get; set; }
}