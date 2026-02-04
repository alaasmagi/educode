using App.Domain.Entities;

namespace App.Contracts.Services;

public interface ICourseManagementService
{
    Task<CourseEntity?> GetCourseByAttendanceIdAsync(Guid attendanceId);
    Task<bool> AddCourse(UserEntity user, CourseEntity course, string creator);
    Task<bool> EditCourse(Guid courseId, CourseEntity newCourse);
    Task<bool> DeleteCourse(Guid courseId, string email);
    Task<List<CourseStatusEntity>?> GetAllCourseStatuses();
    Task<List<CourseEntity>?> GetCoursesByUserAsync(Guid userId, int pageNr, int pageSize);
    // TODO: Move mapping to DTOs to another layer Task<List<AttendanceStudentCountDto>?> GetAttendancesUserCountsByCourseAsync(Guid courseId);
    Task<CourseEntity?> GetCourseByIdAsync(Guid courseId, string email);
    Task<bool> DoesCourseExistByIdAsync(Guid id);
    Task<bool> SeedCourseStatuses();
}