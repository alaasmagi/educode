namespace App.Contracts.WebRequests;

public class ChangePasswordRequest
{
    public Guid UserId { get; set; }
    public required string ClientApp { get; set; }
    public required string Email { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}