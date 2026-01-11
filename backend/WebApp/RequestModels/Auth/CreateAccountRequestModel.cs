namespace WebApp.Models;

public class CreateAccountRequestModel : BaseModel
{
    public required string Fullname { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string UserRole { get; set; }
    public string? StudentCode { get; set; }
}