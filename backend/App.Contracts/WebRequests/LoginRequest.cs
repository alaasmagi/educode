namespace App.Contracts.WebRequests;

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ClientApp { get; set; }
}