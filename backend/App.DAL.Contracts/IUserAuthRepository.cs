using App.Domain;

namespace App.DAL.Contracts;

public interface IUserAuthRepository : IRepository<UserAuthEntity>
{
    Task<UserAuthEntity?> GetByUser(Guid userId, bool includeDeleted = false);
}