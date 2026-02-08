using App.Contracts.DTOs;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface IAttendanceService
{
    Task<MethodResponse<AttendanceDto>> GetCurrentAttendanceAsync(Guid userId);
    Task<MethodResponse<AttendanceDto>> GetAttendanceByIdAsync(Guid id, string email);
    Task<MethodResponse<List<AttendanceDto>>> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize);
    Task<MethodResponse<int>> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier);
    Task<MethodResponse<AttendanceDto>> GetMostRecentAttendanceByUserAsync(Guid userId);
    Task<MethodResponse<bool>> AddAttendanceAsync(AttendanceRequest attendance, string email, string clientApp);
    Task<MethodResponse<bool>> EditAttendanceAsync(Guid id, AttendanceChangeRequest request, string email, string clientApp);
    Task<MethodResponse<bool>> SoftDeleteAttendanceAsync(Guid id, string email, string clientApp);
}