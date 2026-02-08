using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTools.Extensions;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceCheckController(
    IAttendanceService attendanceService,
    IAttendanceCheckService attendanceCheckService,
    IUserService userService,
    ILogger<AttendanceCheckController> logger)
    : ControllerBase
{
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<AttendanceCheckDto>>> GetAttendanceCheckById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        
        var response = await attendanceCheckService.GetAttendanceCheckByIdAsync(id);
        if (!response.Successful)
        {
            return NotFound(response.Error);
        }
        
        var result = new AttendanceCheckDto(attendanceCheck);
        
        logger.LogInformation($"Attendance checks for attendance with ID {id} successfully fetched");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
    [HttpPost]
    public async Task<IActionResult> AddAttendanceCheck([FromBody] AttendanceCheckRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new {message = "Invalid credentials", messageCode = "invalid-credentials"});
        }
        
        var newAttendanceCheck = new AttendanceCheckEntity
        {
            StudentCode = request.StudentCode,
            FullName = request.FullName,
            AttendanceIdentifier = request.CourseAttendanceIdentifier,
            CreatedBy = request.Client,
            UpdatedBy = request.Client,
        };

        if (!await attendanceCheckService.AddAttendanceCheckAsync(newAttendanceCheck, request.WorkplaceIdentifier ?? null, 
                                                                                                            request.Client))
        {
            return BadRequest(new {message = "Attendance check already exists", 
                messageCode = "attendance-check-already-exists" });
        }

        logger.LogInformation($"Attendance check added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAttendanceCheck(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        if (!await attendanceCheckService.DeleteAttendanceCheck(id, userId.ToGuid().ToString(), client))
        {
            return BadRequest(new { message = "AttendanceCheck does not exist", 
                messageCode = "attendance-check-does-not-exist" });
        }
        
        logger.LogInformation($"Attendance check with ID {id} deleted successfully");
        return Ok();
    }
}