namespace App.Contracts.WebRequests;

public class OtpRequest : BaseRequest
{
    public required string Email { get; set; }
    public string? FullName { get; set; }
}