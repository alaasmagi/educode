namespace Base.Contracts.DataAccess;

public interface IBaseUow
{
    public Task<int> SaveChangesAsync();
}