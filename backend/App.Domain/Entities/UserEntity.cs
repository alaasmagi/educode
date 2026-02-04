using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class UserEntity : BaseEntity
{
    [Required]
    public Guid TypeId { get; set; }
    public UserTypeEntity? Type { get; set; }
    public Guid? SchoolId { get; set; }
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