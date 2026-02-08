using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Base.Domain;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController(
    IOtpService otpService,
    IUserService userService,
    IAuthService authService,
    EnvInitializer envInitializer,
    ILogger<OtpController> logger)
    : ControllerBase
{

    [HttpPost("Request")]
    public async Task<IActionResult> RequestOtp([FromBody] OtpRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }

        var response = await authService.GenerateAndSendOtpAsync(request);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"OTP sent successfully for user with email {request.Email}");
        return Ok();
    }

    [HttpPost("Verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var creatorIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }

        var response = await authService.VerifyOtpAsync(request, creatorIp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        Response.Cookies.Append("token", response.Value.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
        });
        
        Response.Cookies.Append("refreshToken", response.Value.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            MaxAge = TimeSpan.FromDays(envInitializer.RefreshTokenCookieExpirationDays)
        });
            
        
        logger.LogInformation($"OTP verified successfully");
        return Ok();
    }
}