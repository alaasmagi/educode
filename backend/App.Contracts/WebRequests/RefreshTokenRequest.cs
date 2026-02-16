namespace App.Contracts.WebRequests;

public class RefreshTokenRequest
{
    public required string ClientApp { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}