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

        await otpService.GenerateAndStoreOtp(request.Email);
        
        logger.LogInformation($"OTP sent successfully for user with email {request.Email}");
        return Ok(new { message = "OTP sent successfully" });
    }

    [HttpPost("Verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }
        
        var user = await userService.GetUserByEmailAsync(request.Email);
        
        var result = await otpService.VerifyOtp(request.Email, request.Otp);

        if (!result)
        {
            return Unauthorized(new { message = "Invalid OTP", messageCode = "invalid-otp" });
        }

        if (user != null)
        {
            var token = authService.GenerateJwtToken(user);
            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
            });
            
            var creatorIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var refreshToken = await authService.GenerateRefreshToken(user.Id, creatorIp, request.Client);
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(envInitializer.RefreshTokenCookieExpirationDays)
            });
                
            logger.LogInformation($"OTP verified successfully for user with email {user.Email}");
            return Ok(new { Token = token });
        }
        
        logger.LogInformation($"OTP verified successfully");
        return Ok();
    }
}