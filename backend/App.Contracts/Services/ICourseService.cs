using App.Contracts.DTOs;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface ICourseService
{
    Task<MethodResponse<CourseDto>> GetCourseByAttendanceIdAsync(Guid attendanceId);
    Task<MethodResponse<List<AttendanceStudentCountDto>>> GetAttendancesUserCountsByCourseIdAsync(Guid id);
    Task<MethodResponse<bool>> AddCourseAsync(Guid userId, CourseRequest request, string email, string clientApp);
    Task<MethodResponse<bool>> EditCourseAsync(Guid id, CourseRequest request, string email, string clientApp);
    Task<MethodResponse<bool>> SoftDeleteCourseAsync(Guid id, string email, string clientApp);
    Task<MethodResponse<List<CourseStatusDto>>> GetAllCourseStatusesAsync();
    Task<MethodResponse<List<CourseDto>>> GetCoursesByUserAsync(Guid userId, int pageNr, int pageSize);
    Task<MethodResponse<CourseDto>> GetCourseByIdAsync(Guid courseId);
}