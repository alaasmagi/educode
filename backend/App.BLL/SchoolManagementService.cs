using System.Text.Json;
using App.BLL.Contracts;
using App.Common;
using App.DAL.Contracts;
using App.DAL.EF;
using App.Domain;
using Microsoft.Extensions.Logging;

namespace App.BLL;

public class SchoolManagementService(ISchoolRepository schoolRepository, RedisRepository redisRepository, 
                                    ILogger<SchoolManagementService> logger, SentryService sentry) : ISchoolManagementService
{
    public async Task<List<SchoolEntity>?> GetAllSchools(int pageNr, int pageSize)
    {
        var cache = await redisRepository.GetAsync(Constants.SchoolPrefix + pageNr + pageSize);
        if (cache != null)
        {
            logger.LogInformation("Successfully retrieved all schools from cache");
            return JsonSerializer.Deserialize<List<SchoolEntity>?>(cache);
        }
        
        logger.LogInformation("Cache miss for all schools, fetching from database");
        var schools = await schoolRepository.GetAllAsync(pageNr, pageSize);

        if (schools == null)
        {
            logger.LogWarning("No schools found in database");
            sentry.CaptureInfo("No schools found in database");
            return null;
        }
        
        logger.LogInformation("Successfully retrieved all schools from database, storing in cache");
        var serializedSchools = JsonSerializer.Serialize(schools);
        await redisRepository.SetAsync(Constants.SchoolPrefix + pageNr + pageSize, 
            serializedSchools, Constants.LongCachePeriod);
        
        return schools;
    }

    public async Task<SchoolEntity?> GetSchoolById(Guid id)
    {
        var cache = await redisRepository.GetAsync(Constants.SchoolPrefix + id);
        if (cache != null)
        {
            logger.LogInformation("Successfully retrieved school from cache by ID {SchoolId}", id);
            return JsonSerializer.Deserialize<SchoolEntity?>(cache);
        }
        
        logger.LogInformation("Cache miss for school by id {SchoolId}", id);
        var school = await schoolRepository.GetByIdAsync(id);
        
        if (school == null)
        {
            logger.LogWarning("No schools found in database with ID {SchoolId}", id);
            sentry.CaptureInfo("No schools found in database with ID {0}", id);
            return null;
        }
        
        logger.LogInformation("Successfully retrieved school from database by ID {SchoolId}, storing in cache", id);
        var serializedSchool = JsonSerializer.Serialize(school);
        await redisRepository.SetAsync(Constants.SchoolPrefix + id, serializedSchool,Constants.LongCachePeriod);

        return school;
    }
}