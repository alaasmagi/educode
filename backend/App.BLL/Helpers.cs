using System.Security.Cryptography;
using System.Text;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace App.BLL;

public class Helpers
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
}