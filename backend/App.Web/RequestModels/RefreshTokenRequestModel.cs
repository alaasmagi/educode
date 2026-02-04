namespace App.Web.RequestModels;

public class RefreshTokenRequestModel : BaseModel
{
    public required string JwtToken { get; set; }
    public required string RefreshToken { get; set; }
}