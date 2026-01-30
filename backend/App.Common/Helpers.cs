using Microsoft.AspNetCore.Authorization;

namespace App.Common;

public static class Helpers
{
    public static int GetAccessLevelFromClaims(AuthorizationHandlerContext context)
    {
        var claimValue = context.User.FindFirst(Constants.AccessLevelClaim)?.Value;
        return int.TryParse(claimValue, out var level) ? level : 0;
    }
    
    public static string GetExtensionFromContentType(string contentType)
    {
        return contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            _ => string.Empty,
        };
    }
    
    public static string[] SplitWords(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return Array.Empty<string>();
    
        return str.Split(';', StringSplitOptions.RemoveEmptyEntries);
    }
}