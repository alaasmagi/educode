using App.Application.Contracts.Services;
using App.Application.Initializers;
using App.Contracts.Services;
using App.Web.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController(
    IOtpService otpService,
    IUserManagementService userManagementService,
    IAuthService authService,
    EnvInitializer envInitializer,
    ILogger<OtpController> logger)
    : ControllerBase
{

    [HttpPost("Request")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        var user = await userManagementService.GetUserByEmailAsync(model.Email);
        if (user == null && string.IsNullOrWhiteSpace(model.FullName))
        {
            return Unauthorized(new { message = "Invalid email", messageCode = "invalid-email" });
        }

        var key = await otpService.GenerateAndStoreOtp(model.Email);
        var recipientEmail = user?.Email ?? model.Email;
        var recipientName = user?.FullName ?? model.FullName ?? "EduCode user";
        
        logger.LogInformation($"OTP sent successfully for user with email {model.Email}");
        return Ok(new { message = "OTP sent successfully" });
    }

    [HttpPost("Verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        var user = await userManagementService.GetUserByEmailAsync(model.Email);
        
        var result = await otpService.VerifyOtp(model.Email, model.Otp);

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
            var refreshToken = await authService.GenerateRefreshToken(user.Id, creatorIp, model.Client);
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