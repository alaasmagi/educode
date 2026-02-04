namespace App.Web.RequestModels;

public class AdminLoginModel
{
    public required string Username { get; set; } = default!;
    public required string Password { get; set; } = default!;
    public string?  Message { get; set; } = default!;
}