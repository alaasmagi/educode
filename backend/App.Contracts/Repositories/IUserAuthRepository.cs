using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IUserAuthRepository : IRepository<UserAuthEntity>
{
    Task<UserAuthEntity?> GetByUserAsync(Guid userId, bool includeDeleted = false);
    Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState);
}