using System.Text.Json;
using App.Domain;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace App.DAL.EF;

public class RedisRepository(IConnectionMultiplexer connection, ILogger<RedisRepository> logger)
{
    private readonly IDatabase _database = connection.GetDatabase();
    private readonly ILogger _logger = logger;

    public async Task<bool> SetDataAsync(string key, string serializedValue, TimeSpan? expiry)
    {
        if (!await _database.StringSetAsync(key, serializedValue, expiry))
        {
            _logger.LogError("RedisRepository - setting data failed");
            return false;
        }
        
        _logger.LogInformation($"RedisRepository - setting data successful");
        return true;
    }
    
    public async Task<string?> GetDataAsync(string key)
    {
        var data = await _database.StringGetAsync(key);
        if (data.IsNullOrEmpty)
        {
            _logger.LogError("RedisRepository - getting data failed");
            return null;
        }

        _logger.LogInformation($"RedisRepository - getting data successful");
        return data;
    }
    
    public async Task<bool> DeleteDataAsync(string key)
    {
        if (!await _database.KeyDeleteAsync(key))
        {
            _logger.LogError("RedisRepository - deleting data failed");
            return false;
        }
        
        _logger.LogInformation($"RedisRepository - deleting data successful");
        return true;
    }
    
    public async Task DeleteKeysByPatternAsync(string pattern)
    {
        var endpoints = _database.Multiplexer.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _database.Multiplexer.GetServer(endpoint);
            
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                await _database.KeyDeleteAsync(key);
                _logger.LogInformation($"Deleted Redis key: {key}");
            }
        }
    }
}