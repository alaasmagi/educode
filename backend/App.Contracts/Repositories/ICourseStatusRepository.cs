using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface ICourseStatusRepository : IRepository<CourseStatusEntity>
{
    Task<List<CourseStatusEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<CourseStatusEntity?> GetByItself(string statusName, bool includeDeleted = false);
}