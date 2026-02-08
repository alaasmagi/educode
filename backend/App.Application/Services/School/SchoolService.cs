using System.Text.Json;
using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Infrastructure.Helpers;
using App.Infrastructure.Redis;
using App.Infrastructure.Sentry;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.School;

public class SchoolService(
    ISchoolRepository schoolRepository, 
    RedisRepository redisRepository, 
    ILogger<SchoolService> logger, 
    SentryService sentry) : ISchoolService
{
    public async Task<MethodResponse<List<SchoolDto>>> GetAllSchools(int pageNr, int pageSize)
    {
        var cache = await redisRepository.GetAsync(Constants.SchoolPrefix + pageNr + pageSize);
        if (cache != null)
        {
            logger.LogInformation("Successfully retrieved all schools from cache");
            var deserializedSchools = JsonSerializer.Deserialize<List<SchoolDto>?>(cache);
            return MethodResponse<List<SchoolDto>>.Success(deserializedSchools!);
        }
        
        logger.LogInformation("Cache miss for all schools, fetching from database");
        var schools = await schoolRepository.GetAllAsync(pageNr, pageSize);

        if (schools == null)
        {
            logger.LogWarning("No schools found in database");
            sentry.CaptureInfo("No schools found in database");
            return MethodResponse<List<SchoolDto>>.Failure(
                new Error(ErrorConstants.SchoolsNotFound, "Schools were not found")
            );
        }
        
        var schoolDtos = SchoolDto.ToDtoList(schools);
        logger.LogInformation("Successfully retrieved all schools from database, storing in cache");
        var serializedSchools = JsonSerializer.Serialize(schoolDtos);
        await redisRepository.SetAsync(Constants.SchoolPrefix + pageNr + pageSize, 
            serializedSchools, Constants.LongCachePeriod);
        
        return MethodResponse<List<SchoolDto>>.Success(schoolDtos);
    }

    public async Task<MethodResponse<SchoolDto>> GetSchoolById(Guid id)
    {
        var cache = await redisRepository.GetAsync(Constants.SchoolPrefix + id);
        if (cache != null)
        {
            logger.LogInformation("Successfully retrieved school from cache by ID {SchoolId}", id);
            var deserializedSchool = JsonSerializer.Deserialize<SchoolDto?>(cache);
            return MethodResponse<SchoolDto>.Success(deserializedSchool!);
        }
        
        logger.LogInformation("Cache miss for school by id {SchoolId}", id);
        var school = await schoolRepository.GetByIdAsync(id);
        
        if (school == null)
        {
            logger.LogWarning("No schools found in database with ID {SchoolId}", id);
            sentry.CaptureInfo("No schools found in database with ID {0}", id);
            return MethodResponse<SchoolDto>.Failure(
                new Error(ErrorConstants.SchoolNotFound, "School was not found")
            );        
        }
        
        var schoolDto = new SchoolDto(school);
        logger.LogInformation("Successfully retrieved school from database by ID {SchoolId}, storing in cache", id);
        var serializedSchool = JsonSerializer.Serialize(schoolDto);
        await redisRepository.SetAsync(Constants.SchoolPrefix + id, serializedSchool,Constants.LongCachePeriod);

        return MethodResponse<SchoolDto>.Success(schoolDto);
    }
}