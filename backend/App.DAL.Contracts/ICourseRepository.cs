using App.Domain;

namespace App.DAL.Contracts;

public interface ICourseRepository : IRepository<CourseEntity>
{
    Task<List<CourseEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}