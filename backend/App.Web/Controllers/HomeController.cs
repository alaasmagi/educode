using System.Diagnostics;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using App.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Controllers;

public class AdminPanelController(
    IAuthService authService, 
    ILogger<AdminPanelController> logger, 
    EnvInitializer envInitializer) : Controller
{
    [HttpGet]
    public IActionResult Index(string? message)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var model = new AdminLoginRequest
        {
            Username = string.Empty,
            Password = string.Empty,
            Message = message ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([Bind("Username", "Password")] AdminLoginRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var clientIp = HttpContext.Connection.RemoteIpAddress!.ToString();

        var loginRequest = new LoginRequest
        {
            Email = request.Username,
            Password = request.Password,
            ClientApp = Constants.BackendName
        };
        var adminUser = await authService.AuthenticateUserAsync(loginRequest, clientIp, 
                                                                        loginRequest.ClientApp,true);
        
        if (!adminUser.Successful)
        {
            return Index("Wrong username or password!");
        }
        
        var (user, token, refreshToken) = adminUser.Value;
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
        });
        
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(envInitializer.RefreshTokenExpirationDays)
        });
        
        logger.LogInformation($"Admin access granted successfully. JWT and refresh token cookies set.");
        return  RedirectToAction("Home");
    }

    public IActionResult LogOut()
    {
        Response.Cookies.Delete("jwt");
        Response.Cookies.Delete("refreshToken");
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RefreshToken()
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var currentRefreshToken = Request.Cookies["refreshToken"];
        var currentAccessToken = Request.Cookies["jwt"];
        
        if (string.IsNullOrEmpty(currentRefreshToken) || string.IsNullOrEmpty(currentAccessToken))
        {
            logger.LogWarning("Missing refresh token or access token in cookies");
            return RedirectToAction("Index", new { message = "Session expired. Please login again." });
        }
        
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        System.IdentityModel.Tokens.Jwt.JwtSecurityToken? jwtToken;
        try
        {
            jwtToken = handler.ReadJwtToken(currentAccessToken);
        }
        catch
        {
            logger.LogWarning("Failed to read JWT token");
            return RedirectToAction("Index", new { message = "Invalid session. Please login again." });
        }
        
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
        if (emailClaim == null)
        {
            logger.LogWarning("Email claim not found in JWT");
            return RedirectToAction("Index", new { message = "Invalid session. Please login again." });
        }
        
        var refreshResult = await authService.RefreshTokensAsync(
            currentRefreshToken, 
            currentAccessToken, 
            emailClaim.Value, 
            Constants.BackendName);
        
        if (!refreshResult.Successful)
        {
            logger.LogWarning($"Token refresh failed: {refreshResult.Error?.Message}");
            return RedirectToAction("Index", new { message = "Session expired. Please login again." });
        }
        
        var (newAccessToken, newRefreshToken) = refreshResult.Value;
        
        Response.Cookies.Append("jwt", newAccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
        });
        
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(envInitializer.RefreshTokenExpirationDays)
        });
        
        logger.LogInformation("Tokens refreshed successfully");
        return RedirectToAction("Home");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Policy = nameof(EAccessLevel.QuaternaryLevel))]
    public IActionResult Home(string? message)
    {
        var model = new AdminLoginRequest
        {
            Username = string.Empty,
            Password = string.Empty,
            Message = message ?? string.Empty
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}