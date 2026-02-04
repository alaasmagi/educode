using App.Domain.Entities;
using App.Domain.Enums;

namespace App.Contracts.Repositories;

public interface IUserTypeRepository : IRepository<UserTypeEntity>
{
    Task<List<UserTypeEntity>?> GetTypeByLevelAsync(EAccessLevel level);
    Task<UserTypeEntity?> GetByItselfAsync(string userType, bool includeDeleted = false);
    Task<List<UserTypeEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}