using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshTokenEntity>
{
    Task<RefreshTokenEntity?> GetByItselfAsync(string refreshToken);
    Task<List<RefreshTokenEntity>?> GetAllByUserAsync(Guid userId);
    Task<bool> RemoveAllByUserAsync(Guid userId);
}