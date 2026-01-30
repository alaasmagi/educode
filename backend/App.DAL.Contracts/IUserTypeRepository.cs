using App.Domain;

namespace App.DAL.Contracts;

public interface IUserTypeRepository : IRepository<UserTypeEntity>
{
    Task<List<UserTypeEntity>?> SearchAsync(string keyword, bool includeDeleted = false);
}