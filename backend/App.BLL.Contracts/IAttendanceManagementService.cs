using App.Domain;

namespace App.BLL.Contracts;

public interface IAttendanceManagementService
{
    Task<AttendanceEntity?> GetCurrentAttendanceAsync(Guid userId);
    Task<AttendanceEntity?> GetCourseAttendanceByIdAsync(Guid attendanceId, string email);
    Task<bool> AddAttendanceCheckAsync(AttendanceCheckEntity attendanceCheck, string creator, string? workplaceIdentifier);
    Task<List<AttendanceEntity>?> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize);
    Task<List<AttendanceCheckEntity>?> GetAttendanceChecksByAttendanceIdAsync(string attendanceIdentifier, int pageNr, int pageSize);
    Task<int> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier);
    Task<AttendanceEntity?> GetMostRecentAttendanceByUserAsync(Guid userId);
    Task<List<AttendanceTypeEntity>?> GetAttendanceTypesAsync();
    Task<AttendanceTypeEntity?> GetAttendanceTypeByIdAsync(Guid attendanceTypeId);
    Task<bool> AddAttendanceAsync(AttendanceEntity newAttendance, List<DateOnly> attendanceDates,
        TimeOnly startTime, TimeOnly endTime);
    Task<AttendanceCheckEntity?> GetAttendanceCheckByIdAsync(Guid id, string email);
    Task<bool> DeleteAttendance(Guid attendanceId, string email);
    Task<bool> EditAttendanceAsync(Guid attendanceId, AttendanceEntity updatedAttendance);
    Task<bool> DeleteAttendanceCheck(Guid attendanceCheckId, string email);
    Task<bool> IsAttendanceAccessibleByUser(AttendanceEntity attendance, string email);
    Task<bool> IsAttendanceCheckAccessibleByUser(AttendanceCheckEntity attendanceCheck, string email);
    void SeedAttendanceTypes();
}