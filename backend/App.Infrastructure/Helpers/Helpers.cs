using Microsoft.AspNetCore.Authorization;

namespace App.Infrastructure.Helpers;
public static class Helpers
{
    public static string[] SplitWords(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return Array.Empty<string>();
    
        return str.Split(';', StringSplitOptions.RemoveEmptyEntries);
    }
    
    public static int GetAccessLevelFromClaims(AuthorizationHandlerContext context)
    {
        var claimValue = context.User.FindFirst(Constants.AccessLevelClaim)?.Value;
        return int.TryParse(claimValue, out var level) ? level : 0;
    }
}