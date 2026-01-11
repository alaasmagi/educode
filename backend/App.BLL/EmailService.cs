using System.Net;
using System.Net.Mail;
using Contracts;
using Microsoft.Extensions.Logging;

namespace App.BLL;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string email, string fullName, int oneTimeKey)
    {
        var mail = Environment.GetEnvironmentVariable("MAILSENDER_EMAIL") ?? "EMAIL";
        var key = Environment.GetEnvironmentVariable("MAILSENDER_KEY") ?? "KEY";
        var host = Environment.GetEnvironmentVariable("MAILSENDER_HOST") ?? "HOST";
        var port = int.Parse(Environment.GetEnvironmentVariable("MAILSENDER_PORT") ?? "0");

        var subject = $"{email} account verification";
        var messageBody = $"Hello, {fullName}! \n\nHere is a one-time key to verify ownership of " +
                          $"Your EduCode account: {oneTimeKey}";
        
        var smtpClient = new SmtpClient(host)
        {
            Port = port,
            Credentials = new NetworkCredential(mail, key),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(mail),
            Subject = subject,
            Body = messageBody,
            IsBodyHtml = false
        };

        mailMessage.To.Add($"{fullName} <{email}>");
        try
        {
            await smtpClient.SendMailAsync(mailMessage);
            return true;
        }
        catch (SmtpException ex)
        {
            _logger.LogError($"Failed to send OTP email: {ex.Message}");
            return false;
        }
    }
}