using App.Domain;

namespace App.DAL.Contracts;

public interface IUserTypeRepository : IRepository<UserTypeEntity>
{
    Task<List<UserTypeEntity>?> GetTypeByLevelAsync(EAccessLevel level);
    Task<UserTypeEntity?> GetByTypeAsync(string userType, bool includeDeleted);
    Task<List<UserTypeEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    void SeedUserTypes(List<UserTypeEntity> userTypes);
}