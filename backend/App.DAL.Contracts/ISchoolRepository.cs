using App.Domain;

namespace App.DAL.Contracts;

public interface ISchoolRepository : IRepository<SchoolEntity>
{
    Task<List<SchoolEntity>?> SearchAsync(string keyword, bool includeDeleted = false);
}