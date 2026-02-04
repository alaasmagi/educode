using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class AttendanceTypeEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string TypeName { get; set; } = default!;
}