namespace App.Contracts.WebRequests;

public class VerifyOtpRequest
{
    public required string ClientApp { get; set; }
    public required string Email { get; set; }
    public required string Otp { get; set; }
}