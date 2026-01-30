using App.Domain;

namespace App.DAL.Contracts;

public interface IWorkplaceRepository : IRepository<WorkplaceEntity>
{
    Task<List<WorkplaceEntity>?> SearchAsync(string keyword, bool includeDeleted = false);
}