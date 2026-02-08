using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using App.Infrastructure.Initializers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserService userService,
    EnvInitializer envInitializer,
    ILogger<AuthController> logger)
    : ControllerBase
{

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestRequest requestRequest)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var user = await userService.GetUserByEmailAsync(requestRequest.Email);

        if (user == null)
        {
            return NotFound(new {message = "User not found", messageCode = "user-not-found"});
        }
        
        var userAuthData = await userService.AuthenticateUserAsync(user.Id, requestRequest.Password);
        if (userAuthData == null || !ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return Unauthorized(new { message = "Invalid email or password", messageCode = "invalid-email-password" });
        }

        var jwtToken = authService.GenerateJwtToken(user);
        var creatorIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var refreshToken = await authService.GenerateRefreshToken(user.Id, creatorIp, requestRequest.Client);

        if (refreshToken == null)
        {
            logger.LogWarning($"Refresh token generation failed");
            return BadRequest(new { message = "Refresh token generation failed", messageCode = "refresh-token-error" });
        }
        
        Response.Cookies.Append("jwt", jwtToken, new CookieOptions
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
        return Ok(new { UserId = user.Id, Token = jwtToken, RefreshToken = refreshToken});
    }
    
    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");

        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var (newJwt,  newRefreshToken) = await authService.RefreshJwtToken(request.RefreshToken, request.JwtToken, ipAddress, request.Client);

        if (newJwt == null || newRefreshToken == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token", messageCode = "invalid-refresh-token" });
        }
        
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

        return Ok(new { Token = newJwt, RefreshToken = newRefreshToken });
    }
    
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
    
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        var status = await authService.DeleteRefreshToken(request.RefreshToken);

        if (status == false)
        {
            logger.LogWarning($"Logging out failed");
            return BadRequest(new { message = "Logging out failed", messageCode = "logout-failed" });
        }

        Response.Cookies.Delete("jwt");
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully", messageCode = "logout-successful" });
    }

    [HttpPost("Register/{token}")]
    public async Task<IActionResult> Register(string? token, [FromBody] CreateAccountRequestRequest requestRequest)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userType = await userService.GetUserTypeAsync(requestRequest.UserRole);
        var newUser = new UserEntity();
        var newUserAuth = new UserAuthEntity();

        if (userType == null || !ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        newUser.Email = requestRequest.Email;
        newUser.FullName = requestRequest.Fullname;
        newUser.StudentCode = requestRequest.StudentCode;
        newUser.TypeId = userType.Id;
        newUser.CreatedBy = requestRequest.Client;
        newUser.UpdatedBy = requestRequest.Client;
        
        newUserAuth.CreatedBy = requestRequest.Client;
        newUserAuth.UpdatedBy = requestRequest.Client;
        newUserAuth.PasswordHash = await authService.HashPasswordAsync(requestRequest.Password);

        if (!await userService.CreateAccountAsync(newUser, newUserAuth))
        {
            return BadRequest(new { message = "User already exists", messageCode = "user-already-exists" });
        }
        
        logger.LogInformation($"User with email {newUser.Email} was created successfully");
        return Ok();
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

        var user = await userService.GetUserByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email", messageCode = "invalid-email" });
        }

        // TODO: Refactor and move the logic to service layer
        var newPasswordHash = await authService.HashPasswordAsync(request.NewPassword);

        if (!await userService.ChangeUserPasswordAsync(user, newPasswordHash))
        {
            return BadRequest(new { message = "Password change error. Password was not changed.", messageCode = "password-not-changed" });
        }

        logger.LogInformation($"Password changed successfully for user with email {request.Email}");
        return Ok(new { message = "Password is changed successfully" });
    }
}
