namespace App.Web.RequestModels;

public class VerifyOtpModel : BaseModel
{
    public required string Email { get; set; }
    public required string Otp { get; set; }
}