using App.Contracts.DTOs;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface IAttendanceService
{
    Task<MethodResponse<AttendanceDto>> GetCurrentAttendanceAsync(Guid userId);
    Task<MethodResponse<AttendanceDto>> GetAttendanceByIdAsync(Guid attendanceId, string email);
    Task<MethodResponse<List<AttendanceDto>>> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize);
    Task<MethodResponse<int>> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier);
    Task<MethodResponse<AttendanceDto>> GetMostRecentAttendanceByUserAsync(Guid userId);

    Task<MethodResponse<bool>> AddAttendanceAsync(AttendanceEntity newAttendance, List<DateOnly> attendanceDates,
        TimeOnly startTime, TimeOnly endTime, string client);
    Task<MethodResponse<bool>> DeleteAttendance(Guid attendanceId, string email, string client);
    Task<MethodResponse<bool>> EditAttendanceAsync(Guid attendanceId, AttendanceEntity updatedAttendance, string client);
}