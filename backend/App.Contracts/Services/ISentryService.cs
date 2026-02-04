namespace App.Application.Contracts.Services;

public interface ISentryService
{
    void CaptureWithContext(Exception ex, string message, params object[] args);
    void CaptureInfo(string message, params object[] args);
}