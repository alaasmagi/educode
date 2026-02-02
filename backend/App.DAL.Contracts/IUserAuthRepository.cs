using App.Domain;

namespace App.DAL.Contracts;

public interface IUserAuthRepository : IRepository<UserAuthEntity>
{
    Task<UserAuthEntity?> GetByUserAsync(Guid userId, bool includeDeleted = false);
}