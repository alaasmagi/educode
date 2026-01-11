using System.Text.Json;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace App.BLL;

public class SchoolManagementService : ISchoolManagementService
{
    private readonly SchoolRepository _schoolRepository;
    private readonly ILogger<SchoolManagementService> _logger;
    private readonly RedisRepository _redisRepository;

    public SchoolManagementService(AppDbContext context, ILogger<SchoolManagementService> logger,
                                    IConnectionMultiplexer connectionMultiplexer, ILogger<RedisRepository> redisLogger)
    {
        _logger = logger;
        _schoolRepository = new SchoolRepository(context);
        _redisRepository = new RedisRepository(connectionMultiplexer, redisLogger); 
    }

    public async Task<List<SchoolEntity>?> GetAllSchools(int pageNr, int pageSize)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.SchoolPrefix + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<SchoolEntity>?>(cache);
        }
        
        var schools = await _schoolRepository.GetAllSchools(pageNr, pageSize);
        
        var serializedSchools = JsonSerializer.Serialize(schools);
        await _redisRepository.SetDataAsync(Constants.SchoolPrefix + pageNr + pageSize, 
            serializedSchools, Constants.LongCachePeriod);
        
        return schools.Count > 0 ? schools : null;
    }

    public async Task<SchoolEntity?> GetSchoolById(Guid id)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.SchoolPrefix + id);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<SchoolEntity?>(cache);
        }
        
        var school = await _schoolRepository.GetSchoolById(id);
        
        var serializedSchool = JsonSerializer.Serialize(school);
        await _redisRepository.SetDataAsync(Constants.SchoolPrefix + id, serializedSchool,Constants.LongCachePeriod);

        return school;
    }
}