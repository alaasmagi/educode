using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class AttendanceTypeEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string AttendanceType { get; set; } = default!;
}