namespace App.Web.RequestModels;

public class RequestOtpModel : BaseModel
{
    public required string Email { get; set; }
    public string? FullName { get; set; }
}