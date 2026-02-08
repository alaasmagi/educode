namespace App.Contracts.WebRequests;

public class RefreshTokenRequest
{
    public required string JwtToken { get; set; }
    public required string RefreshToken { get; set; }
}