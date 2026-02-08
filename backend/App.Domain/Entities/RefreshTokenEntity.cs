using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.DTO;

namespace App.Domain.Entities;

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

    public override List<Error> Validate()
    {
        var errors = base.Validate();

        if (UserId == Guid.Empty)
        {
            errors.Add(new Error("user-id-empty", "UserId cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(Token))
        {
            errors.Add(new Error("token-empty", "Token cannot be empty"));
        }
        else if (Token.Length > 256)
        {
            errors.Add(new Error("token-too-long", "Token cannot exceed 256 characters"));
        }

        if (!string.IsNullOrWhiteSpace(PushNotificationToken) && PushNotificationToken.Length > 256)
        {
            errors.Add(new Error("push-notification-token-too-long", "PushNotificationToken cannot exceed 256 characters"));
        }

        if (string.IsNullOrWhiteSpace(Client))
        {
            errors.Add(new Error("client-empty", "Client cannot be empty"));
        }
        else if (Client.Length > 128)
        {
            errors.Add(new Error("client-too-long", "Client cannot exceed 128 characters"));
        }

        if (string.IsNullOrWhiteSpace(ClientIp))
        {
            errors.Add(new Error("client-ip-empty", "ClientIp cannot be empty"));
        }
        else if (ClientIp.Length > 128)
        {
            errors.Add(new Error("client-ip-too-long", "ClientIp cannot exceed 128 characters"));
        }

        if (ExpirationTime == default)
        {
            errors.Add(new Error("expiration-time-empty", "ExpirationTime cannot be empty"));
        }

        return errors;
    }
}