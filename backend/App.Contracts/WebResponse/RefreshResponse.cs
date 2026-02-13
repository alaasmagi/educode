namespace App.Contracts.WebResponse;

public class RefreshResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}