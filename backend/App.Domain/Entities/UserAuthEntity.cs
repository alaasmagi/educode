using Base.Domain;

namespace App.Domain.Entities;

public class UserAuthEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }
    public string PasswordHash { get; set; } = default!;
    public bool Verified { get; set; } = false;
}