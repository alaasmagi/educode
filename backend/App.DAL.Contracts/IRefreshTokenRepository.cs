using App.Domain;

namespace App.DAL.Contracts;

public interface IRefreshTokenRepository : IRepository<RefreshTokenEntity>
{
    Task<RefreshTokenEntity?> GetByItselfAsync(string refreshToken);
}