using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain;

public class RefreshTokenEntity : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    [MaxLength(256)]
    public string Token { get; set; } = default!;
    [MaxLength(256)]
    public string? PushNotificationToken { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string Client { get; set; } = default!;
    [Required]
    [MaxLength(128)]
    public string ClientIp { get; set; } = default!;
    [Required]
    public DateTime ExpirationTime { get; set; }
}