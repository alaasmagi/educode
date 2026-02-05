using System.Diagnostics;
using App.Application.Initializers;
using App.Contracts.Services;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Web.Models;
using App.Web.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Controllers;

public class AdminPanelController(
    IAuthService authService, 
    IAccessTokenService accessTokenService,
    ILogger<AdminPanelController> logger, 
    EnvInitializer envInitializer) : Controller
{
    [HttpGet]
    public IActionResult Index(string? message)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var model = new AdminLoginModel
        {
            Username = string.Empty,
            Password = string.Empty,
            Message = message ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([Bind("Username", "Password")] AdminLoginModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var clientIp = HttpContext.Connection.RemoteIpAddress!.ToString();
        
        var adminUser = await authService.AuthenticateUserAsync(model.Username, model.Password,
                                                                clientIp, Constants.BackendName,true);
        
        if (adminUser == null)
        {
            return Index("Wrong username or password!");
        }
        
        var (user, _, _) = adminUser.Value;
        
        var token = accessTokenService.GenerateAccessToken(user, null);
        
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
        });
        
        logger.LogInformation($"Admin access granted successfully. JWT cookie set.");
        return  RedirectToAction("Home");
    }

    public IActionResult LogOut()
    {
        Response.Cookies.Delete("jwt");
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize(Policy = nameof(EAccessLevel.QuaternaryLevel))]
    public IActionResult Home(string? message)
    {
        var model = new AdminLoginModel
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