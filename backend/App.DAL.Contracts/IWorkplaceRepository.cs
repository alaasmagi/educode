using App.Domain;

namespace App.DAL.Contracts;

public interface IWorkplaceRepository : IRepository<WorkplaceEntity>
{
    Task<List<WorkplaceEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByIdentifierAsync(string identifier, bool includeDeleted = false);
}