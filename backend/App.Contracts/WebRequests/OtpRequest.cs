namespace App.Contracts.WebRequests;

public class OtpRequest
{
    public required string Email { get; set; }
    public string? FullName { get; set; }
}