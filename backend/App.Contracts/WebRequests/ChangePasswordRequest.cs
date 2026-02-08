namespace App.Contracts.WebRequests;

public class ChangePasswordRequest : BaseRequest
{
    public required string Email { get; set; }
    public required string NewPassword { get; set; }
}