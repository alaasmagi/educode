using System.Text.Json;
using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.AttendanceCheck;

public class AttendanceCheckService(
    ILogger<AttendanceCheckService> logger,
    IAttendanceRepository attendanceRepository,
    ICacheRepository cacheRepository,
    IWorkplaceRepository workplaceRepository,
    IAttendanceCheckRepository attendanceCheckRepository) : IAttendanceCheckService
{
    public async Task<MethodResponse<bool>> AddAttendanceCheckAsync(AttendanceCheckEntity attendanceCheck, string? workplaceIdentifier, string client)
    {
        AttendanceCheckEntity? status;
        attendanceCheck.StudentCode = attendanceCheck.StudentCode.ToUpper();
        if (workplaceIdentifier != null)
        {
            var workplaceId = await workplaceRepository.CheckAvailabilityByIdentifierAsync(workplaceIdentifier);
            
            if (workplaceId == null)
            {
                logger.LogWarning($"Workplace with identifier {workplaceIdentifier} was not found");
                return MethodResponse<bool>.Failure(
                    new Error(ErrorConstants.WorkplaceNotFound, "Workplace was not found")
                );
            }
            
            status = await attendanceCheckRepository.CreateAsync(attendanceCheck);
        }
        else
        {
            status = await attendanceCheckRepository.CreateAsync(attendanceCheck);
        }
        
        if (status == null)
        {
            logger.LogWarning($"Attendance check adding failed");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AttendanceCheckNotCreated, "Attendance check was not created")
            );
        }
        
        logger.LogInformation($"Successfully added attendance check");
        return MethodResponse<bool>.Success(true);
    }
    
    public async Task<MethodResponse<List<AttendanceCheckDto>>> GetAttendanceChecksByAttendanceIdAsync(string attendanceIdentifier, 
                                                                                                int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + attendanceIdentifier + pageNr + pageSize);
        if (cache != null)
        {
            var deserializedChecks = JsonSerializer.Deserialize<List<AttendanceCheckDto>?>(cache);
            return MethodResponse<List<AttendanceCheckDto>>.Success(deserializedChecks!);
        }
        
        var attendanceId = await attendanceRepository.CheckAvailabilityByIdentifierAsync(attendanceIdentifier);
        
        if (attendanceId == null)
        {
            logger.LogWarning($"Attendance with identifier {attendanceIdentifier} was not found");
            return MethodResponse<List<AttendanceCheckDto>>.Failure(
                new Error(ErrorConstants.AttendanceNotFound, "Attendance was not found")
            );
        }
        
        var attendanceChecks = await attendanceCheckRepository.GetAllByAttendanceAsync(attendanceId.Value);
        
        if (attendanceChecks == null)
        {
            logger.LogWarning($"Attendance checks for attendance with identifier {attendanceIdentifier} were not found");
            return MethodResponse<List<AttendanceCheckDto>>.Failure(
                new Error(ErrorConstants.AttendanceChecksNotFound, "Attendance checks were not found")
            );
        }
        
        var attendanceCheckDtos = AttendanceCheckDto.ToDtoList(attendanceChecks);
        var serializedAttendanceCheckDtos = JsonSerializer.Serialize(attendanceCheckDtos);
        await cacheRepository.SetAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + attendanceIdentifier + pageNr + pageSize, 
            serializedAttendanceCheckDtos, Constants.ShortCachePeriod);
        
        logger.LogInformation($"Successfully retrieved attendance check by attendance with ID {attendanceId}");
        return MethodResponse<List<AttendanceCheckDto>>.Success(attendanceCheckDtos);
    }
    
    public async Task<MethodResponse<AttendanceCheckDto>> GetAttendanceCheckByIdAsync(Guid attendanceCheckId)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceCheckPrefix + attendanceCheckId);

        if (cache != null)
        {
            var deserializedCheck = JsonSerializer.Deserialize<AttendanceCheckDto?>(cache);
            return MethodResponse<AttendanceCheckDto>.Success(deserializedCheck!);
        }
        
        var attendanceCheck = await attendanceCheckRepository.GetByIdAsync(attendanceCheckId);
        if (attendanceCheck == null)
        {
            logger.LogWarning($"AttendanceCheck with ID {attendanceCheckId} was not found");
            return MethodResponse<AttendanceCheckDto>.Failure(
                new Error(ErrorConstants.AttendanceCheckNotFound, "Attendance check was not found")
            );
        }

        var attendanceCheckDto = new AttendanceCheckDto(attendanceCheck);
        var serializedAttendanceDto = JsonSerializer.Serialize(attendanceCheckDto);
        await cacheRepository.SetAsync(Constants.AttendanceCheckPrefix + attendanceCheckId, serializedAttendanceDto, Constants.MediumCachePeriod);
        
        
        var result = new AttendanceCheckDto(attendanceCheck);
        logger.LogInformation($"Successfully retrieved attendance check with ID {attendanceCheckId}");
        return MethodResponse<AttendanceCheckDto>.Success(result);
    }
    
    public async Task<MethodResponse<bool>> DeleteAttendanceCheck(Guid attendanceCheckId, string email, string client)
    {
        var attendanceCheck = await attendanceCheckRepository.GetByIdAsync(attendanceCheckId);
        if (attendanceCheck == null)
        {
            logger.LogWarning($"Failed to delete attendance check with ID {attendanceCheckId}: check not found");
            return MethodResponse<bool>.Failure(                    
                new Error(ErrorConstants.AttendanceCheckNotFound, "Attendance check was not found")
            );
        }
        
        await cacheRepository.DeletePatternAsync($"*{attendanceCheckId.ToString()}*");
        var status = await attendanceCheckRepository.RemoveAsync(attendanceCheck);

        if (status == null)
        {
            logger.LogWarning($"Failed to delete attendance check with ID {attendanceCheckId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AttendanceCheckNotDeleted, "Attendance check was not deleted")
            );
        }
        
        logger.LogInformation($"Successfully deleted attendance check with ID {attendanceCheckId}");
        return MethodResponse<bool>.Success(true);
    }
}