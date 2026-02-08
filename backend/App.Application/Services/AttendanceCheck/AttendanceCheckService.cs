using System.Text.Json;
using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using Base.Domain;
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
    public async Task<MethodResponse<bool>> AddAttendanceCheckAsync(AttendanceCheckRequest request, string? email, string clientApp)
    {
        var newAttendanceCheck = new AttendanceCheckEntity
        {
            StudentCode = request.StudentCode,
            FullName = request.FullName,
            AttendanceIdentifier = request.AttendanceIdentifier,
            CreatedBy = request.IsOffline ? Constants.OfflineUser : email ?? Constants.BackendName,
            CreatedByClient = clientApp,
            UpdatedBy = request.IsOffline ? Constants.OfflineUser : email ?? Constants.BackendName,
            UpdatedByClient = clientApp
        };
        
        newAttendanceCheck.StudentCode = newAttendanceCheck.StudentCode.ToUpper();
        AttendanceCheckEntity? status;
        if (request.WorkplaceIdentifier != null)
        {
            var workplaceId = await workplaceRepository.CheckAvailabilityByIdentifierAsync(request.WorkplaceIdentifier);
            
            if (workplaceId == null)
            {
                logger.LogWarning($"Workplace with identifier {request.WorkplaceIdentifier} was not found");
                return MethodResponse<bool>.Failure(
                    new Error(ErrorConstants.WorkplaceNotFound, "Workplace was not found")
                );
            }
            
            status = await attendanceCheckRepository.CreateAsync(newAttendanceCheck);
        }
        else
        {
            status = await attendanceCheckRepository.CreateAsync(newAttendanceCheck);
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
    
    public async Task<MethodResponse<List<AttendanceCheckDto>>> GetAttendanceChecksByAttendanceIdAsync(Guid id, int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + id + pageNr + pageSize);
        if (cache != null)
        {
            var deserializedChecks = JsonSerializer.Deserialize<List<AttendanceCheckDto>?>(cache);
            return MethodResponse<List<AttendanceCheckDto>>.Success(deserializedChecks!);
        }
        
        var attendanceChecks = await attendanceCheckRepository.GetAllByAttendanceAsync(id);
        if (attendanceChecks == null)
        {
            logger.LogWarning($"Attendance checks for attendance with identifier {id} were not found");
            return MethodResponse<List<AttendanceCheckDto>>.Failure(
                new Error(ErrorConstants.AttendanceChecksNotFound, "Attendance checks were not found")
            );
        }
        
        var attendanceCheckDtos = AttendanceCheckDto.ToDtoList(attendanceChecks);
        var serializedAttendanceCheckDtos = JsonSerializer.Serialize(attendanceCheckDtos);
        await cacheRepository.SetAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + id + pageNr + pageSize, 
            serializedAttendanceCheckDtos, Constants.ShortCachePeriod);
        
        logger.LogInformation($"Successfully retrieved attendance check by attendance with ID {id}");
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
    
    public async Task<MethodResponse<bool>> SoftDeleteAttendanceCheckAsync(Guid id, string email, string clientApp)
    {
        var attendanceCheck = await attendanceCheckRepository.GetByIdAsync(id);
        if (attendanceCheck == null)
        {
            logger.LogWarning($"Failed to delete attendance check with ID {id}: check not found");
            return MethodResponse<bool>.Failure(                    
                new Error(ErrorConstants.AttendanceCheckNotFound, "Attendance check was not found")
            );
        }
        
        await cacheRepository.DeletePatternAsync($"*{id.ToString()}*");
        var status = await attendanceCheckRepository.ToggleDeletionAsync(id, email, clientApp, true);

        if (!status)
        {
            logger.LogWarning($"Failed to delete attendance check with ID {id}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AttendanceCheckNotDeleted, "Attendance check was not deleted")
            );
        }
        
        logger.LogInformation($"Successfully deleted attendance check with ID {id}");
        return MethodResponse<bool>.Success(true);
    }
}