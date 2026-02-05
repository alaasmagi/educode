using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<List<UserEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<UserEntity?> GetByEmailAsync(string email, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByEmailAsync(string email, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByFullNameAsync(string fullName, bool includeDeleted = false);
    Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState);
}