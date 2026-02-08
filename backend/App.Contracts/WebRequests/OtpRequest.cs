namespace App.Contracts.WebRequests;

public class OtpRequest
{
    public required string ClientApp { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
}