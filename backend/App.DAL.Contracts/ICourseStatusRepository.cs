using App.Domain;

namespace App.DAL.Contracts;

public interface ICourseStatusRepository : IRepository<CourseStatusEntity>
{
    Task<List<CourseStatusEntity>?> SearchAsync(string keyword, bool includeDeleted = false);
}