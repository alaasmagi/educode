using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class GeneralController(
    ILogger<GeneralController> logger) : ControllerBase
{

    [HttpGet("HealthCheck")]
    public IActionResult CheckHealth()
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        return Ok(DateTime.UtcNow);
    }
}