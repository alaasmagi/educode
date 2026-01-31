using App.Domain;

namespace App.DAL.Contracts;

public interface ISchoolRepository : IRepository<SchoolEntity>
{
    Task<List<SchoolEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}