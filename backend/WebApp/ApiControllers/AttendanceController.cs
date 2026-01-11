using System.Security.Claims;
using App.BLL;
using App.Domain;
using App.DTO;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(
    IAttendanceManagementService attendanceManagementService,
    ICourseManagementService courseManagementService,
    IUserManagementService userManagementService,
    ILogger<AttendanceController> logger)
    : ControllerBase
{
    [Authorize(Policy = nameof(EAccessLevel.SecondaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseAttendanceDto>> GetAttendanceById(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        var attendanceEntity = await attendanceManagementService.GetCourseAttendanceByIdAsync(id, userId);

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
        var courseEntity = await courseManagementService.GetCourseByAttendanceIdAsync(id);

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
        var attendanceTypes = await attendanceManagementService.GetAttendanceTypesAsync();

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
    public async Task<ActionResult> AddCourseAttendance([FromBody] AttendanceModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        var course = await courseManagementService.GetCourseByIdAsync(model.CourseId, userId);
        if (course == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var attendanceType = await attendanceManagementService.GetAttendanceTypeByIdAsync(model.AttendanceTypeId);
        if (attendanceType == null)
        {
            return NotFound(new {message = "Attendance type not found", messageCode = "attendance-type-not-found"});
        }
        
        var newAttendance = new CourseAttendanceEntity()
        {
            CourseId = model.CourseId,
            AttendanceTypeId = model.AttendanceTypeId,
            CreatedBy = model.Client,
            UpdatedBy = model.Client
        };
        if (!await attendanceManagementService.AddAttendanceAsync(newAttendance, model.AttendanceDates, model.StartTime,
                model.EndTime))
        {
            return BadRequest(new {message = "One or more attendances could not be added", 
                messageCode = "attendances-could-not-be-added"});
        }
        
        logger.LogInformation($"Attendance added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPatch("{id}")]
    public async Task<ActionResult> EditAttendance(Guid id, [FromBody] AttendanceModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid || model.Id == null)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        var course = await courseManagementService.GetCourseByIdAsync(model.CourseId, userId);
        if (course == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var attendanceType = await attendanceManagementService.GetAttendanceTypeByIdAsync(model.AttendanceTypeId);
        if (attendanceType == null)
        {
            return NotFound(new {message = "Attendance type not found", messageCode = "attendance-type-not-found"});
        }
        
        var newAttendance = new CourseAttendanceEntity()
        {
            CourseId = model.CourseId,
            AttendanceTypeId = model.AttendanceTypeId,
            StartTime = model.AttendanceDates[0].ToDateTime(model.StartTime).ToUniversalTime(),
            EndTime = model.AttendanceDates[0].ToDateTime(model.EndTime).ToUniversalTime(),
            CreatedBy = model.Client,
            UpdatedBy = model.Client
        };

        var attendanceId = model.Id.Value;
        if (!await attendanceManagementService.EditAttendanceAsync(attendanceId, newAttendance))
        {
            return BadRequest(new { message = "Attendance does not exist", messageCode = "attendance-does-not-exist" });
        }

        logger.LogInformation($"Attendance for attendance with ID {model.Id} updated successfully");
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
        
        if (!await attendanceManagementService.DeleteAttendance(id, userId))
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
       
        var courseAttendance = await attendanceManagementService.GetCourseAttendanceByIdAsync(id, userId);

        if (courseAttendance == null)
        {
            return NotFound(new { message = "Attendance not found", messageCode = "attendance-not-found" });
        }

        var attendanceChecks =
            await attendanceManagementService.GetAttendanceChecksByAttendanceIdAsync(courseAttendance.Identifier, pageNr, pageSize);
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