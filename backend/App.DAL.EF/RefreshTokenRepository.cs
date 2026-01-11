using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class RefreshTokenRepository (AppDbContext context)
{
    public async Task<RefreshTokenEntity?> GetRefreshToken(string refreshToken)
    {
        return await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken);
    }
    
    public async Task<bool> AddRefreshTokenEntityToDb(RefreshTokenEntity refreshToken)
    {
        refreshToken.CreatedAt = DateTime.UtcNow;
        refreshToken.UpdatedAt = DateTime.UtcNow;
        
        await context.RefreshTokens.AddAsync(refreshToken);
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteRefreshTokenEntity(RefreshTokenEntity refreshToken)
    {
        context.RefreshTokens.Remove(refreshToken);
        return await context.SaveChangesAsync() > 0;
    }
}