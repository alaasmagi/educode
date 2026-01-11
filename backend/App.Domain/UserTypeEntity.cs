using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Domain;

namespace App.Domain;

public class UserTypeEntity : BaseEntity
{
    [Required]
    [MaxLength(128)]
    public string UserType { get; set; } = default!;
    [Required] 
    public EAccessLevel AccessLevel { get; set; } = EAccessLevel.NoAccess;
}