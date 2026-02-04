using System.ComponentModel.DataAnnotations;
using App.Domain.Enums;
using Base.Domain;

namespace App.Domain.Entities;

public class UserTypeEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string TypeName { get; set; } = default!;
    [Required] 
    public EAccessLevel AccessLevel { get; set; } = EAccessLevel.NoAccess;
}