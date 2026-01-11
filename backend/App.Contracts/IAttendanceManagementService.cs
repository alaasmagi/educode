using App.Domain;

namespace Contracts;

public interface IAttendanceManagementService
{
    Task<CourseAttendanceEntity?> GetCurrentAttendanceAsync(Guid userId);
    Task<CourseAttendanceEntity?> GetCourseAttendanceByIdAsync(Guid attendanceId, string email);
    Task<bool> AddAttendanceCheckAsync(AttendanceCheckEntity attendanceCheck, string creator, string? workplaceIdentifier);
    Task<bool> DoesAttendanceCheckExist(string studentCode, string fullName, string attendanceIdentifier);
    Task<bool> DoesWorkplaceExist(string workplaceIdentifier);
    Task<List<CourseAttendanceEntity>?> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize);
    Task<List<AttendanceCheckEntity>?> GetAttendanceChecksByAttendanceIdAsync(string attendanceIdentifier, int pageNr, int pageSize);
    Task<int> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier);
    Task<CourseAttendanceEntity?> GetMostRecentAttendanceByUserAsync(Guid userId);
    Task<List<AttendanceTypeEntity>?> GetAttendanceTypesAsync();
    Task<AttendanceTypeEntity?> GetAttendanceTypeByIdAsync(Guid attendanceTypeId);
    Task<bool> AddAttendanceAsync(CourseAttendanceEntity newAttendance, List<DateOnly> attendanceDates,
        TimeOnly startTime, TimeOnly endTime);
    Task<AttendanceCheckEntity?> GetAttendanceCheckByIdAsync(Guid id, string email);
    Task<bool> DeleteAttendance(Guid attendanceId, string email);
    Task<bool> EditAttendanceAsync(Guid attendanceId, CourseAttendanceEntity updatedAttendance);
    Task<bool> DeleteAttendanceCheck(Guid attendanceCheckId, string email);
    Task<bool> IsAttendanceAccessibleByUser(CourseAttendanceEntity attendance, string email);
    Task<bool> IsAttendanceCheckAccessibleByUser(AttendanceCheckEntity attendanceCheck, string email);
}