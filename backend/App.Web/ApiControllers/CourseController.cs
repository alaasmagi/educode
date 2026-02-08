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
public class CourseController(
    ICourseService courseService,
    IAttendanceService attendanceService,
    IUserService userService,
    ILogger<CourseController> logger)
    : ControllerBase
{
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourseDetails(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        var courseEntity = await courseService.GetCourseByIdAsync(id, userId!);

        if (courseEntity == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var result = new CourseDto(courseEntity);
        
        logger.LogInformation($"Successfully fetched course by course ID {id}");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}/Attendances")]
    public async Task<ActionResult<IEnumerable<CourseAttendanceDto>>> GetAttendancesByCourseId(Guid id, [FromQuery] int pageNr = 1, 
                                                                        [FromQuery] int pageSize = Constants.DefaultQueryPageSize)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        var course = await courseService.GetCourseByIdAsync(id, userId);

        if (course == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var attendances = 
            await attendanceService.GetAttendancesByCourseAsync(course.Id, pageNr, pageSize);

        if (attendances == null)
        {
            return Ok(new {message = "Course has no attendances", messageCode = "no-course-attendances-found"});
        }
        
        var result = CourseAttendanceDto.ToDtoList(attendances);
        
        logger.LogInformation($"Attendances for course {id} successfully fetched");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("Statuses")]
    public async Task<IActionResult> GetAllCourseStatuses()
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var courseStatuses = await courseService.GetAllCourseStatuses();

        if (courseStatuses == null)
        {
            return NotFound(new {message = "Course statuses not found", messageCode = "course-statuses-not-found"});
        }
        
        var result = CourseStatusDto.ToDtoList(courseStatuses);
        
        logger.LogInformation($"All course statuses fetched successfully");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}/StudentCounts")]
    public async Task<ActionResult<IEnumerable<AttendanceStudentCountDto>>> GetAllStudentCountsByCourse(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var validity = await courseService.DoesCourseExistByIdAsync(id);
        if(!validity)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var result = await courseService.GetAttendancesUserCountsByCourseAsync(id);

        if (result == null)
        {
            return NotFound(new {message = "No student counts found", messageCode = "student-counts-not-found"});
        }
        
        logger.LogInformation($"All student counts for course with ID {id}");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPost]
    public async Task<ActionResult> AddCourse([FromBody] CourseRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        var user = await userService.GetUserByIdAsync(Guid.Parse(userId));
        if (user == null)
        {
            return BadRequest(new { message = "User does not exist", messageCode = "user-not-found" });
        }
        
        var newCourse = new CourseEntity
        {
            Name = request.CourseName,
            Code = request.CourseCode,
            StatusId = request.CourseStatusId,
            CreatedBy = request.Client,
            UpdatedBy = request.Client,
        };

        if (!await courseService.AddCourse(user, newCourse, request.Client))
        {
            
            return BadRequest(new { message = "Course already exists", messageCode = "course-already-exists" });
        }
        
        logger.LogInformation($"Course added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPatch("{id}")]
    public async Task<ActionResult> EditCourse(Guid id, [FromBody] CourseRequest request)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        if (!ModelState.IsValid || request.Id == null)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        var newCourse = new CourseEntity
        {
            Name = request.CourseName,
            Code = request.CourseCode,
            StatusId = request.CourseStatusId,
            CreatedBy = request.Client,
            UpdatedBy = request.Client,
        };

        var courseId = request.Id.Value;
        if (!await courseService.EditCourse(courseId, newCourse))
        {
            return BadRequest(new { message = "Course does not exist", messageCode = "course-does-not-exist" });
        }
        
        logger.LogInformation($"Course with ID {request.Id} updated successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCourse(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        if (!await courseService.DeleteCourse(id, userId!))
        {
            return BadRequest(new { message = "Course does not exist", messageCode = "course-does-not-exist" });
        }
        
        logger.LogInformation($"Course with ID {id} deleted successfully");
        return Ok();
    }
}