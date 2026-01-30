using App.Domain;

namespace App.DAL.Contracts;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<List<UserEntity>?> SearchAsync(string keyword, bool includeDeleted = false);
}