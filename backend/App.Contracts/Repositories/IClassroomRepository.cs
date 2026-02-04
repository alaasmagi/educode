using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IClassroomRepository : IRepository<ClassroomEntity>
{
    Task<List<ClassroomEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}