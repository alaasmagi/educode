namespace App.Common;
public class SentryService(IHub sentry)
{
    public void CaptureWithContext(Exception ex, string message, params object[] args)
    {
        sentry.CaptureException(ex, scope =>
        {
            scope.AddBreadcrumb(string.Format(message, args));
        });
    }
    
    public void CaptureInfo(string message, params object[] args)
    {
        sentry.CaptureMessage(string.Format(message, args));
    }
}