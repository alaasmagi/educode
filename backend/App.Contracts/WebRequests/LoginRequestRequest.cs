namespace App.Contracts.WebRequests;

public class LoginRequestRequest : BaseRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}