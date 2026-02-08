using App.Contracts.WebRequests;

namespace App.Contracts.Services;

public interface IEmailService
{
    Task<bool> SendOtpAsync(OtpEmailApiRequest request);
}