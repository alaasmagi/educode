namespace WebApp.Models;

public class LoginRequestModel : BaseModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}