namespace App.Contracts.WebRequests;

public class CreateAccountRequest
{
    public required string ClientApp { get; set; }
    public required string Fullname { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required Guid SchoolId { get; set; }
    public string? StudentCode { get; set; }
}