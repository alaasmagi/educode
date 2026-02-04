using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface ICourseRepository : IRepository<CourseEntity>
{
    Task<List<CourseEntity>?> GetAllByUserAsync(Guid userId, int pageNr, int pageSize, bool includeDeleted = false);
    Task<List<(Guid, DateTime, int)>?> GetUserCountsAsync(Guid id);
    Task<List<CourseEntity>?> GetAllSingleUserByUserAsync(Guid userId, bool includeDeleted = false);
    Task<List<CourseEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByCodeAsync(string code, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByNameAsync(string name, bool includeDeleted = false);
    Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState);
}