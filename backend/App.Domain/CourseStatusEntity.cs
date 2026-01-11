using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class CourseStatusEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string CourseStatus { get; set; } = default!;
}