namespace Base.Contracts.Application;

public interface IBaseServiceUow
{
    public Task<int> SaveChangesAsync();
}