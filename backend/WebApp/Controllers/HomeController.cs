using System.Diagnostics;
using App.BLL;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.Auth;
using WebApp.RequestModels.Auth;

namespace WebApp.Controllers;

public class AdminPanelController(IAuthService authService, ILogger<AdminPanelController> logger, EnvInitializer envInitializer)
    : Controller
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
    public IActionResult Index([Bind("Username", "Password")] AdminLoginModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");

        var token = authService.AdminAccessGrant(model.Username, model.Password);
        if (token == null)
        {
            return Index("Wrong username or password!");
        }
        
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromMinutes(envInitializer.JwtCookieExpirationMinutes)
        });
        
        logger.LogInformation($"Admin access granted successfully");
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