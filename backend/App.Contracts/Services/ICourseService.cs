using App.Contracts.DTOs;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface ICourseService
{
    Task<MethodResponse<CourseDto>> GetCourseByAttendanceIdAsync(Guid attendanceId);
    Task<MethodResponse<bool>> AddCourse(UserEntity user, CourseEntity course, string client);
    Task<MethodResponse<bool>> EditCourse(Guid courseId, CourseEntity newCourse, string client);
    Task<MethodResponse<bool>> DeleteCourse(Guid courseId, string email, string client);
    Task<MethodResponse<List<CourseStatusDto>>> GetAllCourseStatuses();
    Task<MethodResponse<List<CourseDto>>> GetCoursesByUserAsync(Guid userId, int pageNr, int pageSize);
    // TODO: Move mapping to DTOs to another layer Task<List<AttendanceStudentCountDto>?> GetAttendancesUserCountsByCourseAsync(Guid courseId);
    Task<MethodResponse<CourseDto>> GetCourseByIdAsync(Guid courseId);
}