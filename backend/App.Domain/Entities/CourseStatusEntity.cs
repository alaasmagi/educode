using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class CourseStatusEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string StatusName { get; set; } = default!;
}