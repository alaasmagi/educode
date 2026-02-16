using App.Contracts.DTOs;

namespace App.Contracts.WebResponse;

public class LoginResponse
{
    public required UserDto User { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}