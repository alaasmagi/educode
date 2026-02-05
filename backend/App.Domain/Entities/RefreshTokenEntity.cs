using Base.Domain;

namespace App.Domain.Entities;

public class RefreshTokenEntity : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public string? PushNotificationToken { get; set; }
    public string Client { get; set; } = default!;
    public string ClientIp { get; set; } = default!;
    public DateTime ExpirationTime { get; set; }
}