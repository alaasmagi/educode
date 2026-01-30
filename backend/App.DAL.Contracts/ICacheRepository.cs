namespace App.DAL.Contracts;

public interface ICacheRepository
{
    Task<string?> GetAsync(string key);
    Task<string?> SetAsync(string key, string serializedValue, TimeSpan? expiry);
    Task<string?> DeleteAsync(string key);
    Task<string?> DeletePatternAsync(string pattern);
}