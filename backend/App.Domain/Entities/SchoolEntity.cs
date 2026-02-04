using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class SchoolEntity : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string ShortName { get; set; } = default!;
    [Required]
    [MaxLength(255)]
    public string Domain { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string StudentCodePattern { get; set; } = default!;
    
    public ICollection<ClassroomEntity>? Classrooms { get; set; }
}