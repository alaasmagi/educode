namespace App.Contracts.WebRequests;

public class VerifyOtpRequest : BaseRequest
{
    public required string Email { get; set; }
    public required string Otp { get; set; }
}