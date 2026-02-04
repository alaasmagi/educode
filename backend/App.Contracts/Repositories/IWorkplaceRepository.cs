using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IWorkplaceRepository : IRepository<WorkplaceEntity>
{
    Task<List<WorkplaceEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByIdentifierAsync(string identifier, bool includeDeleted = false);
}