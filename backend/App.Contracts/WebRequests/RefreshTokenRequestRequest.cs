namespace App.Contracts.WebRequests;

public class RefreshTokenRequestRequest : BaseRequest
{
    public required string JwtToken { get; set; }
    public required string RefreshToken { get; set; }
}