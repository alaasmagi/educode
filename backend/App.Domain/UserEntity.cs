using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Domain;

namespace App.Domain;

public class UserEntity : BaseEntity
{
    [Required]
    public Guid UserTypeId { get; set; }
    public UserTypeEntity? UserType { get; set; }
    [Required]
    public Guid SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    [Required]
    [MaxLength(128)]
    public string Email { get; set; } = default!;
    [MaxLength(128)]
    public string? StudentCode { get; set; }
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = default!;
    [MaxLength(255)]
    public string? PhotoPath { get; set; } = default!;
    public List<RefreshTokenEntity>? RefreshTokens { get; set; } = default!;
}