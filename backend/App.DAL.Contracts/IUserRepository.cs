using App.Domain;

namespace App.DAL.Contracts;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<List<UserEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByEmailAsync(string email, bool includeDeleted);
    void SeedAdminUser(UserEntity adminUser, UserAuthEntity adminAuth);
}