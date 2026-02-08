using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Base.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController(
    ICourseService courseService,
    IAttendanceService attendanceService,
    ILogger<CourseController> logger)
    : ControllerBase
{
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourseById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");

        var response = await courseService.GetCourseByIdAsync(id);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"Successfully fetched course by course ID {id}");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}/Attendances")]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendancesByCourseId(Guid id, [FromQuery] int pageNr = 1, 
                                                                        [FromQuery] int pageSize = Constants.DefaultQueryPageSize)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = 
            await attendanceService.GetAttendancesByCourseAsync(id, pageNr, pageSize);

        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"Attendances for course {id} successfully fetched");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("Statuses")]
    public async Task<IActionResult> GetAllCourseStatuses()
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await courseService.GetAllCourseStatusesAsync();
        if (!response.Successful)
        {
            return NotFound(response.Error);
        }
        
        logger.LogInformation($"All course statuses fetched successfully");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}/StudentCounts")]
    public async Task<ActionResult<IEnumerable<AttendanceStudentCountDto>>> GetAllStudentCountsByCourse(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await courseService.GetAttendancesUserCountsByCourseIdAsync(id);
        if (!response.Successful)
        {
            return NotFound(response.Error);
        }
        
        logger.LogInformation($"Successfully retrieved student counts for course with ID {id}");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPost]
    public async Task<ActionResult> AddCourse([FromBody] CourseRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }

        var response = await courseService.AddCourseAsync(new Guid(userId), request, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"Course added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPatch("{id}")]
    public async Task<ActionResult> EditCourse(Guid id, [FromBody] CourseRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        { 
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }
        
        var response = await courseService.EditCourseAsync(id, request, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"Course with ID {id} updated successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCourse(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }

        var response = await courseService.SoftDeleteCourseAsync(id, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"Course with ID {id} deleted successfully");
        return Ok();
    }
}