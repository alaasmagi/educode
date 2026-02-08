using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

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

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (CourseId == Guid.Empty)
        {
            errors.Add(new Error("course-id-empty", "CourseId cannot be empty"));
        }

        if (TeacherId == Guid.Empty)
        {
            errors.Add(new Error("teacher-id-empty", "TeacherId cannot be empty"));
        }

        return errors;
    }
}