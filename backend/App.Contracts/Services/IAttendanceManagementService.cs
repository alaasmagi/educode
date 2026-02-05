using App.Domain.Entities;

namespace App.Contracts.Services;

public interface IAttendanceManagementService
{
    Task<AttendanceEntity?> GetCurrentAttendanceAsync(Guid userId);
    Task<AttendanceEntity?> GetCourseAttendanceByIdAsync(Guid attendanceId, string email);
    Task<bool> AddAttendanceCheckAsync(AttendanceCheckEntity attendanceCheck, string? workplaceIdentifier, string client);
    Task<List<AttendanceEntity>?> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize);
    Task<List<AttendanceCheckEntity>?> GetAttendanceChecksByAttendanceIdAsync(string attendanceIdentifier, int pageNr, int pageSize);
    Task<int> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier);
    Task<AttendanceEntity?> GetMostRecentAttendanceByUserAsync(Guid userId);
    Task<List<AttendanceTypeEntity>?> GetAttendanceTypesAsync();
    Task<AttendanceTypeEntity?> GetAttendanceTypeByIdAsync(Guid attendanceTypeId);
    Task<bool> AddAttendanceAsync(AttendanceEntity newAttendance, List<DateOnly> attendanceDates,
        TimeOnly startTime, TimeOnly endTime, string client);
    Task<AttendanceCheckEntity?> GetAttendanceCheckByIdAsync(Guid id, string email);
    Task<bool> DeleteAttendance(Guid attendanceId, string email, string client);
    Task<bool> EditAttendanceAsync(Guid attendanceId, AttendanceEntity updatedAttendance, string client);
    Task<bool> DeleteAttendanceCheck(Guid attendanceCheckId, Guid userId, string client);
    Task<bool> IsAttendanceAccessibleByUser(AttendanceEntity attendance, string email);
    Task<bool> IsAttendanceCheckAccessibleByUser(AttendanceCheckEntity attendanceCheck, string email);
    Task<bool> SeedAttendanceTypes();

}