namespace App.Contracts.WebRequests;

public class OtpEmailApiRequest
{
    public string EmailTo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public int OtpExpirationMinutes { get; set; }
}