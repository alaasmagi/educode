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
public class CourseController(
    ICourseManagementService courseManagementService,
    IAttendanceManagementService attendanceManagementService,
    IUserManagementService userManagementService,
    ILogger<CourseController> logger)
    : ControllerBase
{
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourseDetails(Guid id)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        var courseEntity = await courseManagementService.GetCourseByIdAsync(id, userId!);

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
        var course = await courseManagementService.GetCourseByIdAsync(id, userId);

        if (course == null)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var attendances = 
            await attendanceManagementService.GetAttendancesByCourseAsync(course.Id, pageNr, pageSize);

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
        var courseStatuses = await courseManagementService.GetAllCourseStatuses();

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
        var validity = await courseManagementService.DoesCourseExistByIdAsync(id);
        if(!validity)
        {
            return NotFound(new {message = "Course not found", messageCode = "course-not-found"});
        }
        
        var result = await courseManagementService.GetAttendancesUserCountsByCourseAsync(id);

        if (result == null)
        {
            return NotFound(new {message = "No student counts found", messageCode = "student-counts-not-found"});
        }
        
        logger.LogInformation($"All student counts for course with ID {id}");
        return Ok(result);
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPost]
    public async Task<ActionResult> AddCourse([FromBody] CourseModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
        if (!ModelState.IsValid)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }

        var user = await userManagementService.GetUserByIdAsync(Guid.Parse(userId));
        if (user == null)
        {
            return BadRequest(new { message = "User does not exist", messageCode = "user-not-found" });
        }
        
        var newCourse = new CourseEntity
        {
            CourseName = model.CourseName,
            CourseCode = model.CourseCode,
            CourseStatusId = model.CourseStatusId,
            CreatedBy = model.Client,
            UpdatedBy = model.Client,
        };

        if (!await courseManagementService.AddCourse(user, newCourse, model.Client))
        {
            
            return BadRequest(new { message = "Course already exists", messageCode = "course-already-exists" });
        }
        
        logger.LogInformation($"Course added successfully");
        return Ok();
    }
    
    [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
    [HttpPatch("{id}")]
    public async Task<ActionResult> EditCourse(Guid id, [FromBody] CourseModel model)
    {
        logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
        if (!ModelState.IsValid || model.Id == null)
        {
            logger.LogWarning($"Form data is invalid");
            return BadRequest(new { message = "Invalid credentials", messageCode = "invalid-credentials" });
        }
        
        var newCourse = new CourseEntity
        {
            CourseName = model.CourseName,
            CourseCode = model.CourseCode,
            CourseStatusId = model.CourseStatusId,
            CreatedBy = model.Client,
            UpdatedBy = model.Client,
        };

        var courseId = model.Id.Value;
        if (!await courseManagementService.EditCourse(courseId, newCourse))
        {
            return BadRequest(new { message = "Course does not exist", messageCode = "course-does-not-exist" });
        }
        
        logger.LogInformation($"Course with ID {model.Id} updated successfully");
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
        
        if (!await courseManagementService.DeleteCourse(id, userId!))
        {
            return BadRequest(new { message = "Course does not exist", messageCode = "course-does-not-exist" });
        }
        
        logger.LogInformation($"Course with ID {id} deleted successfully");
        return Ok();
    }
}