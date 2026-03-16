using Base.Contracts.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Base.DataAccess.EF;

public class BaseUow<TDbContext> : IBaseUow
    where TDbContext : DbContext
{
    protected readonly TDbContext UowDbContext;

    public BaseUow(TDbContext uowDbContext)
    {
        UowDbContext = uowDbContext;
    }


    public async Task<int> SaveChangesAsync()
    {
        return await UowDbContext.SaveChangesAsync();
    }
}
