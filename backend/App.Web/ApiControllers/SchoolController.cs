using App.Application;
using App.Application.DTOs;
using App.Application.Initializers;
using App.Contracts.Services;
using App.Infrastructure.Helpers;
using App.Infrastructure.Sentry;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolController(
    EnvInitializer envInitializer,
    ISchoolManagementService schoolManagementService,
    ILogger<SchoolController> logger,
    SentryService sentryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SchoolDto>>> GetAllSchools([FromQuery] int pageNr = 1, 
                                                            [FromQuery] int pageSize = Constants.DefaultQueryPageSize)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var schools = await schoolManagementService.GetAllSchools(pageNr, pageSize); 
        if (schools == null)
        {
            return NotFound(new { message = "Schools not found", messageCode = "schools-not-found" });
        }

        var result = SchoolDto.ToDtoList(schools, envInitializer.OciPublicUrl);

        logger.LogInformation($"{schools.Count} schools successfully fetched");
        return result;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<SchoolDto>> GetSchoolById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var school = await schoolManagementService.GetSchoolById(id); 
        if (school == null)
        {
            return NotFound(new { message = $"School with ID {id} not found", messageCode = "school-not-found" });
        }

        var result = new SchoolDto(school, envInitializer.OciPublicUrl);

        logger.LogInformation($"School with ID {id} successfully fetched");
        return result;
    }
}