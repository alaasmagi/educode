namespace WebApp.RequestModels.Otp;

public class VerifyOtpModel : BaseModel
{
    public required string Email { get; set; }
    public required string Otp { get; set; }
}