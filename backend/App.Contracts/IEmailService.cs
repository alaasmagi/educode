namespace Contracts;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string email, string fullName, int oneTimeKey);
}