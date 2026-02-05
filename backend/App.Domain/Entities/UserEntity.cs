using Base.Domain;

namespace App.Domain.Entities;

public class UserEntity : BaseEntity
{
    public Guid TypeId { get; set; }
    public UserTypeEntity? Type { get; set; }
    public Guid? SchoolId { get; set; }
    public SchoolEntity? School { get; set; }
    public string Email { get; set; } = default!;
    public string? StudentCode { get; set; }
    public string FullName { get; set; } = default!;
    public string? PhotoPath { get; set; } = default!;
    public List<RefreshTokenEntity>? RefreshTokens { get; set; } = default!;
}