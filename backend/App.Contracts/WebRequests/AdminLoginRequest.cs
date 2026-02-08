namespace App.Contracts.WebRequests;

public class AdminLoginRequest
{
    public required string Username { get; set; } = default!;
    public required string Password { get; set; } = default!;
    public string?  Message { get; set; } = default!;
}