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
public class AttendanceCheckController(
    IAttendanceCheckService attendanceCheckService,
    ILogger<AttendanceCheckController> logger) : ControllerBase
{
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<AttendanceCheckDto>>> GetAttendanceCheckById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        
        var response = await attendanceCheckService.GetAttendanceCheckByIdAsync(id);
        if (!response.Successful)
        {
            return NotFound(response.Error);
        }
        
        logger.LogInformation($"Attendance checks for attendance with ID {id} successfully fetched");
        return Ok(response.Value);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
    [HttpPost]
    public async Task<IActionResult> AddAttendanceCheck([FromBody] AttendanceCheckRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }
        
        var response = await attendanceCheckService.AddAttendanceCheckAsync(request, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }

        logger.LogInformation($"Attendance check added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAttendanceCheck(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
        var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new Error(ErrorConstants.InvalidCredentials, "Invalid credentials"));
        }

        var response = await attendanceCheckService.SoftDeleteAttendanceCheckAsync(id, email, clientApp);
        if (!response.Successful)
        {
            return BadRequest(response.Error);
        }
        
        logger.LogInformation($"Attendance check with ID {id} deleted successfully");
        return Ok();
    }
}