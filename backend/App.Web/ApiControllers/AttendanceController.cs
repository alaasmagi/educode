using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(
    IAttendanceService attendanceService,
    ICourseService courseService,
    ILogger<AttendanceController> logger)
    : ControllerBase
{
    [Authorize(Policy = nameof(EAccessLevel.SecondaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseAttendanceDto>> GetAttendanceById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        var attendanceEntity = await attendanceService.GetCourseAttendanceByIdAsync(id, userId);

        if (attendanceEntity == null)
        {
            return NotFound(new {message = "Attendance not found", messageCode = "attendance-not-found"});
        }

        var result = new CourseAttendanceDto(attendanceEntity);
        
        logger.LogInformation($"Attendance with ID {id} successfully fetched");
        return result;
    }
    
    [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
    [HttpGet("{id}/Course")]
    public async Task<ActionResult<CourseDto>> GetCourseByAttendanceId(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var courseEntity = await courseService.GetCourseByAttendanceIdAsync(id);

        if (courseEntity == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var result = new CourseDto(courseEntity);
        
        logger.LogInformation($"Successfully fetched course by attendance with ID {id}");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("Types")]
    public async Task<ActionResult<IEnumerable<AttendanceTypeDto>>> GetAllAttendanceTypes()
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var attendanceTypes = await attendanceService.GetAttendanceTypesAsync();

        if (attendanceTypes == null)
        {
            return NotFound(new {message = "Attendance types not found", messageCode = "attendance-types-not-found"});
        }
        
        var result = AttendanceTypeDto.ToDtoList(attendanceTypes);
        
        logger.LogInformation($"All attendance types successfully fetched");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPost]
    public async Task<ActionResult> AddCourseAttendance([FromBody] AttendanceRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        var course = await courseService.GetCourseByIdAsync(request.CourseId, userId);
        if (course == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var attendanceType = await attendanceService.GetAttendanceTypeByIdAsync(request.AttendanceTypeId);
        if (attendanceType == null)
        {
            return NotFound(new {message = "Attendance type not found", messageCode = "attendance-type-not-found"});
        }
        
        var newAttendance = new AttendanceEntity()
        {
            CourseId = request.CourseId,
            TypeId = request.AttendanceTypeId,
            CreatedBy = request.Client,
            UpdatedBy = request.Client
        };
        if (!await attendanceService.AddAttendanceAsync(newAttendance, request.AttendanceDates, request.StartTime,
                request.EndTime, request.Client))
        {
            return BadRequest(new {message = "One or more attendances could not be added", 
                messageCode = "attendances-could-not-be-added"});
        }
        
        logger.LogInformation($"Attendance added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPatch("{id}")]
    public async Task<ActionResult> EditAttendance(Guid id, [FromBody] AttendanceRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid || request.Id == null)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        var course = await courseService.GetCourseByIdAsync(request.CourseId, userId);
        if (course == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var attendanceType = await attendanceService.GetAttendanceTypeByIdAsync(request.AttendanceTypeId);
        if (attendanceType == null)
        {
            return NotFound(new {message = "Attendance type not found", messageCode = "attendance-type-not-found"});
        }
        
        var newAttendance = new AttendanceEntity()
        {
            CourseId = request.CourseId,
            TypeId = request.AttendanceTypeId,
            StartTime = request.AttendanceDates[0].ToDateTime(request.StartTime).ToUniversalTime(),
            EndTime = request.AttendanceDates[0].ToDateTime(request.EndTime).ToUniversalTime(),
            CreatedBy = request.Client,
            UpdatedBy = request.Client
        };

        var attendanceId = request.Id.Value;
        if (!await attendanceService.EditAttendanceAsync(attendanceId, newAttendance, request.Client))
        {
            return BadRequest(new { message = "Attendance does not exist", messageCode = "attendance-does-not-exist" });
        }

        logger.LogInformation($"Attendance for attendance with ID {request.Id} updated successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAttendance(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        if (!await attendanceService.DeleteAttendance(id, userId, ))
        {
            return BadRequest(new { message = "Attendance does not exist", messageCode = "attendance-does-not-exist" });
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
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
       
        var courseAttendance = await attendanceService.GetCourseAttendanceByIdAsync(id, userId);

        if (courseAttendance == null)
        {
            return NotFound(new { message = "Attendance not found", messageCode = "attendance-not-found" });
        }

        var attendanceChecks =
            await attendanceService.GetAttendanceChecksByAttendanceIdAsync(courseAttendance.Identifier, pageNr, pageSize);
        if (attendanceChecks == null)
        {
            return Ok(new
                { message = "Attendance has no attendance checks", messageCode = "attendance-has-no-checks" });
        }

        var result = AttendanceCheckDto.ToDtoList(attendanceChecks);

        logger.LogInformation($"Attendance checks for attendance with ID {id} successfully fetched");
        return Ok(result);
    }
}