using System.Text.Json;
using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Infrastructure.Helpers;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.AttendanceType;

public class AttendanceTypeService(
    ICacheRepository cacheRepository,
    IAttendanceTypeRepository attendanceTypeRepository,
    ILogger<AttendanceTypeService> logger) : IAttendanceTypeService
{
    public async Task<MethodResponse<List<AttendanceTypeDto>>> GetAttendanceTypesAsync()
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceTypePrefix);
        
        if (cache != null)
        {
            var deserializedTypes = JsonSerializer.Deserialize<List<AttendanceTypeDto>?>(cache);
            return MethodResponse<List<AttendanceTypeDto>>.Success(deserializedTypes!);
        }
        
        var result = await attendanceTypeRepository.GetAllAsync(1, 100);
        if (result == null)
        {
            logger.LogWarning($"Failed to get attendance types");
            return MethodResponse<List<AttendanceTypeDto>>.Failure(
                new Error(ErrorConstants.AttendanceTypesNotFound, "Attendance types were not found")
            );
        }
        
        var attendanceTypeDtos = AttendanceTypeDto.ToDtoList(result);
        var serializedAttendanceTypeDtos = JsonSerializer.Serialize(attendanceTypeDtos);
        await cacheRepository.SetAsync(Constants.AttendanceTypePrefix, 
            serializedAttendanceTypeDtos, Constants.ExtraLongCachePeriod);
        
        logger.LogInformation($"Successfully retrieved the attendance types");
        return MethodResponse<List<AttendanceTypeDto>>.Success(attendanceTypeDtos);
    }

    public async Task<MethodResponse<AttendanceTypeDto>> GetAttendanceTypeByIdAsync(Guid attendanceTypeId)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceTypePrefix + attendanceTypeId);
        
        if (cache != null)
        {
            var deserializedType = JsonSerializer.Deserialize<AttendanceTypeDto?>(cache);
            return MethodResponse<AttendanceTypeDto>.Success(deserializedType!);
        }
        
        var result = await attendanceTypeRepository.GetByIdAsync(attendanceTypeId);
        
        if (result == null)
        {
            logger.LogWarning($"Attendance type with ID {attendanceTypeId} was not found");
            return MethodResponse<AttendanceTypeDto>.Failure(
                new Error(ErrorConstants.AttendanceTypeNotFound, "Attendance type was not found")
            );
        }
        
        var attendanceTypeDto = new AttendanceTypeDto(result);
        var serializedAttendanceTypeDto = JsonSerializer.Serialize(attendanceTypeDto);
        await cacheRepository.SetAsync(Constants.AttendanceTypePrefix + attendanceTypeId, 
            serializedAttendanceTypeDto, Constants.ExtraLongCachePeriod);
        
        logger.LogInformation($"Successfully fetched attendance type with ID {attendanceTypeId}");
        return MethodResponse<AttendanceTypeDto>.Success(attendanceTypeDto);
    }
}