using Base.Contracts.Application;
using Base.Contracts.DataAccess;

namespace Base.Application;

public class BaseServiceUow<TUow> : IBaseServiceUow where TUow : IBaseUow
{
    protected readonly TUow Uow;

    public BaseServiceUow(TUow uow)
    {
        Uow = uow;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Uow.SaveChangesAsync();
    }
}