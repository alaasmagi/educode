using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class UserAuthEntity : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = default!;
    [Required] 
    public bool Verified { get; set; } = false;
}