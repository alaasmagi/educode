using App.Application;
using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using App.Infrastructure.Sentry;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolController(
    EnvInitializer envInitializer,
    ISchoolService schoolService,
    ILogger<SchoolController> logger,
    SentryService sentryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SchoolDto>>> GetAllSchools([FromQuery] int pageNr = 1, 
                                                            [FromQuery] int pageSize = Constants.DefaultQueryPageSize)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await schoolService.GetAllSchoolsAsync(pageNr, pageSize); 
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"{response.Value!.Count} schools successfully fetched");
        return Ok(response.Value);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolDto>> GetSchoolById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await schoolService.GetSchoolByIdAsync(id); 
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"School with ID {id} successfully fetched");
        return Ok(response.Value);
    }
}