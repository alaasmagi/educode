namespace App.Contracts.WebRequests;

public class UserRequest
{
    public required string Fullname { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? StudentCode { get; set; }
}