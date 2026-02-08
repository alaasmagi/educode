using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(
        IUserService userService,
        ICourseService courseService,
        IAttendanceService attendanceService,
        IPhotoService photoService,
        ILogger<UserController> logger)
        : ControllerBase
    {
        [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] int pageNr = 1,
            [FromQuery] int pageSize = Constants.DefaultQueryPageSize)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            
            var response = await userService.GetAllUsersAsync(pageNr, pageSize);
            if (!response.Successful)
            {
                return NotFound(response.Error);
            }
            
            logger.LogInformation($"All users fetched successfully");
            return Ok(response.Value);
        }

        [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserEntity(Guid id)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            
            var response = await userService.GetUserByIdAsync(id);            
            if (!response.Successful)
            {
                return NotFound(response.Error);
            }
            
            logger.LogInformation($"User with ID {id} fetched successfully");
            return Ok(response.Value);
        }

        [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUserEntity(Guid id, [FromBody] UserRequest request)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            var response = await userService.UpdateUserAsync(request, email, clientApp);
            if (!response.Successful)
            {
                logger.LogInformation($"Failed to update User with ID {id}");
                return BadRequest(response.Error);
            }

            return Ok();
        }

        [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserEntity(Guid id)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            var email = User.FindFirst(Constants.EmailClaim)?.Value ?? string.Empty;
            var clientApp = User.FindFirst(Constants.ClientAppClaim)?.Value ?? string.Empty;

            var response = await userService.SoftDeleteUserAsync(id, email, clientApp);
            if (!response.Successful)
            {
                return BadRequest(response.Error);
            }

            return Ok();
        }

        [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
        [HttpGet("{id}/Courses")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCoursesByUser(Guid id, [FromQuery] int pageNr = 1,
            [FromQuery] int pageSize = Constants.DefaultQueryPageSize)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");

            var response = await courseService.GetCoursesByUserAsync(id, pageNr, pageSize);
            if (!response.Successful)
            {
                return Ok(response.Error);
            }
            
            logger.LogInformation($"Courses for user with ID {id}");
            return Ok(response.Value);
        }

        [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
        [HttpGet("{id}/CurrentAttendance")]
        public async Task<ActionResult<AttendanceDto>> GetCurrenAttendance(Guid id)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            
            var response = await attendanceService.GetCurrentAttendanceAsync(id);
            if (!response.Successful)
            {
                return BadRequest(response.Error);
            }
            
            logger.LogInformation($"Current attendance for user with ID {id} successfully fetched");
            return Ok(response.Value);
        }

        [Authorize(Policy = nameof(EAccessLevel.TertiaryLevel))]
        [HttpGet("{id}/RecentAttendance")]
        public async Task<ActionResult<AttendanceDto>> GetMostRecentAttendance(Guid id)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            
            var response = await attendanceService.GetMostRecentAttendanceByUserAsync(id);
            if (!response.Successful)
            {
                return BadRequest(response.Error);
            }
            
            logger.LogInformation($"Most recent attendance for user with ID {id} successfully fetched");
            return Ok(response.Value);
        }

        [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
        [HttpPost("{id}/UploadPhoto")]
        public async Task<ActionResult> UploadUserPhoto(Guid id)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
            var user = await userService.GetUserByIdAsync(Guid.Parse(userId));

            if (user == null)
            {
                return NotFound(new { message = "User not found", messageCode = "user-not-found" });
            }

            var file = HttpContext.Request.Form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded", messageCode = "no-file-uploaded" });
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return BadRequest(new
                    { message = "Invalid file type. Only images allowed.", messageCode = "invalid-file-type" });
            }

            if (file.Length > Constants.MaxPictureFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5MB limit.", messageCode = "file-too-large" });
            }

            using (var photoStream = file.OpenReadStream())
            {
                var objectPath = await photoService.UploadPhotoAsync(
                    Constants.UserFolder,
                    id,
                    photoStream,
                    file.ContentType);

                if (objectPath == null)
                {
                    return StatusCode(500,
                        new
                        {
                            message = "Photo upload failed due to server error.", messageCode = "oci-upload-failed"
                        });
                }

                user.PhotoPath = objectPath;
                await userService.UpdateUserAsync(user);
                logger.LogInformation($"Successfully uploaded photo for user {userId}. Path: {objectPath}");
                return Ok(new { message = "Photo uploaded successfully", path = objectPath });
            }
        }

        [Authorize(Policy = nameof(EAccessLevel.PrimaryLevel))]
        [HttpDelete("{id}/RemovePhoto")]
        public async Task<ActionResult> RemoveUserPhoto(Guid id)
        {
            logger.LogInformation($"{HttpContext.Request.Method.ToUpper()} - {HttpContext.Request.Path}");
            var userId = User.FindFirst(Constants.UserIdClaim)?.Value ?? string.Empty;
            var user = await userService.GetUserByIdAsync(Guid.Parse(userId));

            if (user == null)
            {
                return NotFound(new { message = "User not found", messageCode = "user-not-found" });
            }

            if (string.IsNullOrWhiteSpace(user.PhotoPath))
            {
                return BadRequest(new { message = "No photo to remove", messageCode = "no-photo-found" });
            }

            bool removed = await photoService.RemovePhotoAsync(user.PhotoPath);

            if (!removed)
            {
                return StatusCode(500,
                    new { message = "Failed to remove photo from storage", messageCode = "oci-delete-failed" });
            }

            user.PhotoPath = string.Empty;
            await userService.UpdateUserAsync(user);

            logger.LogInformation($"Photo removed for user {user.Id}. Path: {user.PhotoPath}");

            return Ok(new { message = "Photo removed successfully" });
        }
    }
}
