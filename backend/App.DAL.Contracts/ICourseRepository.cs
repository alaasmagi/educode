using App.Domain;
using App.DTO;

namespace App.DAL.Contracts;

public interface ICourseRepository : IRepository<CourseEntity>
{
    Task<List<CourseEntity>?> GetAllByUser(Guid userId, int pageNr, int pageSize, bool includeDeleted = false);
    Task<List<CourseEntity>?> GetAllSingleUserByUser(Guid userId, int pageNr, int pageSize, bool includeDeleted = false);
    Task<List<AttendanceStudentCountDto>?> GetUserCounts(Guid id);
    Task<List<CourseEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByCodeAsync(string code, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByNameAsync(string name, bool includeDeleted = false);
    void SeedCourseStatuses(List<CourseStatusEntity> courseStatuses);
}