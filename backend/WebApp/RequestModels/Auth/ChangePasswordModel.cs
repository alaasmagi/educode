namespace WebApp.RequestModels.Auth;

public class ChangePasswordModel : BaseModel
{
    public required string Email { get; set; }
    public required string NewPassword { get; set; }
}