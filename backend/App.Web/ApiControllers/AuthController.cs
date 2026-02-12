using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Base.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    EnvInitializer envInitializer,
    ILogger<AuthController> logger)
    : ControllerBase
{

    [HttpPost("Login")]
    public async Task<ActionResult<(UserDto, string, string)>> Login([FromBody] LoginRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var creatorIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }
        
        var response = await authService.AuthenticateUserAsync(request, creatorIp, request.ClientApp, true);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        var (user, accessToken, refreshToken) = response.Value;
        Response.Cookies.Append("jwt", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
        });
        
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,                
            Secure = true,                 
            SameSite = SameSiteMode.None,   
            MaxAge = TimeSpan.FromDays(envInitializer.RefreshTokenCookieExpirationDays)
        });
        
        logger.LogInformation($"User with ID {user.Id} was logged in successfully");
        return Ok(response.Value);
    }
    
    [HttpPost("Refresh")]
    public async Task<ActionResult<(string, string)>> Refresh([FromBody] RefreshTokenRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");

        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var response = await authService.RefreshTokensAsync(request.RefreshToken, request.AccessToken, ipAddress, request.ClientApp);

        if (!response.Successful)
        {
            return Unauthorized(response.Error);
        }
        
        var (newJwt, newRefreshToken) = response.Value;
        Response.Cookies.Append("jwt", newJwt, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
        });
        
        Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,                
            Secure = true,                 
            SameSite = SameSiteMode.None,   
            MaxAge = TimeSpan.FromDays(envInitializer.RefreshTokenCookieExpirationDays)
        });

        return Ok(response.Value);
    }
    
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var response = await authService.LogOutUserAsync(refreshToken);

        if (!response.Successful)
        {
            logger.LogWarning($"Logging out failed");
            return BadRequest(response.Error);
        }

        Response.Cookies.Delete("jwt");
        Response.Cookies.Delete("refreshToken");

        return Ok();
    }

    [HttpPost("Register/{token?}")]
    public async Task<ActionResult<UserDto>> Register(string? token, [FromBody] CreateAccountRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        var response = await authService.RegisterUserAsync(request);
        if (!response.Successful)
        {
            return BadRequest(new { message = "User already exists", messageCode = "user-already-exists" });
        }
        
        logger.LogInformation($"User with email {request.Email} was created successfully");
        return Ok(response.Value);
    }

    [Authorize]
    [HttpPatch("ChangePassword")]
    public async Task<IActionResult> ChangeAccountPassword([FromBody] ChangePasswordRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        var response = await authService.ChangePasswordAsync(request);
        if (!response.Successful)
        {
            return Unauthorized(response.Error);
        }

        logger.LogInformation($"Password changed successfully for user with email {request.Email}");
        return Ok();
    }
}
