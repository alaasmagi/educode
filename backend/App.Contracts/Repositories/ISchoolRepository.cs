using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface ISchoolRepository : IRepository<SchoolEntity>
{
    Task<List<SchoolEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}