using App.Contracts.Repositories;
using App.Infrastructure.Sentry;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace App.Infrastructure.Redis;

public class RedisRepository(IDatabase database, ILogger<RedisRepository> logger, SentryService sentry) : ICacheRepository
{
    private static readonly SemaphoreSlim RedisOperationSemaphore = new(16, 16);
    
    // private readonly IDatabase _database = connection.GetDatabase();
    public async Task<string?> GetAsync(string key)
    {
        try
        {
            var data = await database.StringGetAsync(key);
            if (data.IsNullOrEmpty)
            {
                return null;
            }

            return data;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting data from Redis. Key: {Key}", key);
            sentry.CaptureWithContext(ex, "Error getting data from Redis. Key: {0}", key);
            return null;
        }
    }
    
    public async Task<string?> SetAsync(string key, string serializedValue, TimeSpan? expiry)
    {
        await RedisOperationSemaphore.WaitAsync();
        try
        {
            if (!await database.StringSetAsync(key, serializedValue, expiry, When.Always, CommandFlags.None))
            {
                return null;
            }
            
            return key;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting data in Redis. Key: {Key}", key);
            sentry.CaptureWithContext(ex, "Error setting data in Redis. Key: {0}", key);
            return null;
        }
        finally
        {
            RedisOperationSemaphore.Release();
        }
    }
    
    public async Task<string?> DeleteAsync(string key)
    {
        await RedisOperationSemaphore.WaitAsync();
        try
        {
            if (!await database.KeyDeleteAsync(key))
            {
                return null;
            }
            
            return key;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting data from Redis. Key: {Key}", key);
            sentry.CaptureWithContext(ex, "Error deleting data from Redis. Key: {0}", key);
            return null;
        }
        finally
        {
            RedisOperationSemaphore.Release();
        }
    }
    
    public async Task<string?> DeletePatternAsync(string pattern)
    {
        await RedisOperationSemaphore.WaitAsync();
        try
        {
            var endpoints = database.Multiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = database.Multiplexer.GetServer(endpoint);
                
                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    await database.KeyDeleteAsync(key);
                }
            }
            return pattern;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting pattern from Redis. Pattern: {Pattern}", pattern);
            sentry.CaptureWithContext(ex, "Error deleting pattern from Redis. Pattern: {0}", pattern);
            return null;
        }
        finally
        {
            RedisOperationSemaphore.Release();
        }
    }
}