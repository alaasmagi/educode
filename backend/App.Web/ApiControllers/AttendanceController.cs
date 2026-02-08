using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Base.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(
    IAttendanceService attendanceService,
    IAttendanceCheckService attendanceCheckService,
    IAttendanceTypeService attendanceTypeService,
    ICourseService courseService,
    ILogger<AttendanceController> logger)
    : ControllerBase
{
    [Authorize(Policy = nameof(EAccessLevel.SecondaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<AttendanceDto>> GetAttendanceById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        
        var response = await attendanceService.GetAttendanceByIdAsync(id, userId);
        if (!response.Successful)
        {
            return NotFound(response.Error);
        }
        
        logger.LogInformation($"Attendance with ID {id} successfully fetched");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
    [HttpGet("{id}/Course")]
    public async Task<ActionResult<CourseDto>> GetCourseByAttendanceId(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await courseService.GetCourseByAttendanceIdAsync(id);
        if (!response.Successful)
        {
            return NotFound(response.Error);
        }
        
        logger.LogInformation($"Successfully fetched course by attendance with ID {id}");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("Types")]
    public async Task<ActionResult<IEnumerable<AttendanceTypeDto>>> GetAllAttendanceTypes()
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await attendanceTypeService.GetAttendanceTypesAsync();
        if (!response.Successful)
        {
            return NotFound(response.Error);
        }
        
        logger.LogInformation($"All attendance types successfully fetched");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPost]
    public async Task<ActionResult> AddAttendance([FromBody] AttendanceRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }
        
        var response = await attendanceService.AddAttendanceAsync(request, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"Attendance added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPatch("{id}")]
    public async Task<ActionResult> EditAttendance(Guid id, [FromBody] AttendanceChangeRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        var response = await attendanceService.EditAttendanceAsync(id, request, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }

        logger.LogInformation($"Attendance for attendance with ID {id} updated successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAttendance(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }

        var response = await attendanceService.SoftDeleteAttendanceAsync(id, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }

        logger.LogInformation($"Attendance with ID {id} deleted successfully");
        return Ok();
    }

    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}/Checks")]
    public async Task<ActionResult<IEnumerable<AttendanceCheckDto>>> GetAttendanceChecksByAttendanceId(
        Guid id, [FromQuery] int pageNr = 1, [FromQuery] int pageSize = Constants.DefaultQueryPageSize)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await attendanceCheckService.GetAttendanceChecksByAttendanceIdAsync(id, pageNr, pageSize);
        if (!response.Successful)
        {
            return Ok(response.Error);
        }
        
        logger.LogInformation($"Attendance checks for attendance with ID {id} successfully fetched");
        return Ok(response.Value);
    }
}