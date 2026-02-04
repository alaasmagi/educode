using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshTokenEntity>
{
    Task<RefreshTokenEntity?> GetByItselfAsync(string refreshToken);
    Task<List<RefreshTokenEntity>?> GetAllByUser(Guid userId);
}