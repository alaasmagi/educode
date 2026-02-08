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

namespace App.Application.Services.Attendance;

public class AttendanceService(
    ICacheRepository cacheRepository, 
    IAttendanceCheckRepository attendanceCheckRepository,
    IAttendanceRepository attendanceRepository,
    ILogger<AttendanceService> logger) : IAttendanceService
{
    public async Task<MethodResponse<AttendanceDto>> GetCurrentAttendanceAsync(Guid userId)
    {
        var cache = await cacheRepository.GetAsync(Constants.CurrentAttendancePrefix +
                                                                                        Constants.UserPrefix + userId);
        if (cache != null)
        {
            var deserializedAttendance = JsonSerializer.Deserialize<AttendanceDto?>(cache);
            return MethodResponse<AttendanceDto>.Success(deserializedAttendance!);
        }
        
        var currentAttendance = await attendanceRepository.GetOngoingByUserAsync(userId);
        if (currentAttendance == null)
        {
            logger.LogWarning($"Current attendance for user with ID {userId} was not found");
            return MethodResponse<AttendanceDto>.Failure(
                new Error(ErrorConstants.AttendanceNotFound, "Current attendance was not found")
            );
        }
        
        var attendanceDto = new AttendanceDto(currentAttendance);
        var serializedAttendance = JsonSerializer.Serialize(attendanceDto);
        await cacheRepository.SetAsync(Constants.CurrentAttendancePrefix +
                                            Constants.UserPrefix + userId, serializedAttendance,Constants.ExtraShortCachePeriod);
        
        logger.LogInformation($"Successfully retrieved current attendance for user with ID {userId}");
        return MethodResponse<AttendanceDto>.Success(attendanceDto);
    }
    
    public async Task<MethodResponse<AttendanceDto>> GetAttendanceByIdAsync(Guid attendanceId, string email)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendancePrefix + attendanceId);
        if (cache != null)
        {
            logger.LogInformation($"Successfully retrieved attendance from cache for ID {attendanceId}");
            var deserializedAttendance = JsonSerializer.Deserialize<AttendanceDto?>(cache);
            return MethodResponse<AttendanceDto>.Success(deserializedAttendance!);
        }
        
        logger.LogInformation($"Cache miss for attendance with ID {attendanceId}, fetching from database");
        var attendance = await attendanceRepository.GetByIdAsync(attendanceId);

        if (attendance == null)
        {
            logger.LogWarning($"Attendance with ID {attendanceId} was not found");
            return MethodResponse<AttendanceDto>.Failure(
                new Error(ErrorConstants.AttendanceNotFound, "Attendance was not found")
            );
        }
        
        var attendanceDto = new AttendanceDto(attendance);
        var serializedAttendance = JsonSerializer.Serialize(attendanceDto);
        await cacheRepository.SetAsync(Constants.AttendancePrefix + attendanceId, serializedAttendance,Constants.MediumCachePeriod);
        logger.LogInformation($"Successfully retrieved attendance by ID {attendanceId}");
        return MethodResponse<AttendanceDto>.Success(attendanceDto);
    }

    public async Task<MethodResponse<int>> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendancePrefix + Constants.StudentCountPrefix + attendanceIdentifier);
        if (cache != null)
        {
            logger.LogInformation($"Successfully retrieved students count from cache for attendance {attendanceIdentifier}");
            return MethodResponse<int>.Success(int.Parse(cache));
        }
        
        logger.LogInformation($"Cache miss for students count for attendance {attendanceIdentifier}, fetching from database");
        var attendanceId = await attendanceRepository.CheckAvailabilityByIdentifierAsync(attendanceIdentifier);
        
        if(attendanceId == null)
        {
            logger.LogWarning($"Attendance with identifier {attendanceIdentifier} was not found");
            return MethodResponse<int>.Failure(
                new Error(ErrorConstants.AttendanceNotFound, "Attendance was not found")
            );
        }
        
        var result = await attendanceCheckRepository.GetUserCountAsync(attendanceId.Value);
        
        if (result == null)
        {
            logger.LogWarning($"No attendance checks found for attendance with identifier {attendanceIdentifier}");
            return MethodResponse<int>.Failure(
                new Error(ErrorConstants.StudentsCountNotFound, "Students count was not found")
            );
        }
        
        await cacheRepository.SetAsync(Constants.AttendancePrefix + Constants.StudentCountPrefix + attendanceIdentifier, result.ToString()!, 
            Constants.ShortCachePeriod);
        
        logger.LogInformation($"Successfully retrieved students count for attendance with identifier {attendanceIdentifier}");
        return MethodResponse<int>.Success(result.Value);
    }
    
    public async Task<MethodResponse<List<AttendanceDto>>> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendancePrefix + Constants.CoursePrefix + courseId + pageNr + pageSize);
        if (cache != null)
        {
            var deserializedAttendances = JsonSerializer.Deserialize<List<AttendanceDto>?>(cache);
            return MethodResponse<List<AttendanceDto>>.Success(deserializedAttendances!);
        }
        
        var attendances = await attendanceRepository.GetAllByCourseAsync(courseId, pageNr, pageSize);

        if (attendances == null)
        {
            logger.LogWarning($"Attendances by course with ID {courseId} were not found");
            return MethodResponse<List<AttendanceDto>>.Failure(
                new Error(ErrorConstants.AttendancesNotFound, "Attendances were not found")
            );
        }

        var attendanceDtos = AttendanceDto.ToDtoList(attendances);
        var serializedAttendancesByCourse = JsonSerializer.Serialize(attendanceDtos);
        await cacheRepository.SetAsync(Constants.AttendancePrefix + Constants.CoursePrefix + courseId + pageNr + pageSize, 
            serializedAttendancesByCourse, Constants.ShortCachePeriod);
        
        logger.LogInformation($"Successfully retrieved attendances by course with ID {courseId}");
        return MethodResponse<List<AttendanceDto>>.Success(attendanceDtos);
    }

    public async Task<MethodResponse<AttendanceDto>> GetMostRecentAttendanceByUserAsync(Guid userId)
    {
        var cache = await cacheRepository.GetAsync(Constants.RecentAttendancePrefix + 
                                                        Constants.UserPrefix + userId);
        if (cache != null)
        {
            var deserializedAttendance = JsonSerializer.Deserialize<AttendanceDto?>(cache);
            return MethodResponse<AttendanceDto>.Success(deserializedAttendance!);
        }
        
        var attendance = await attendanceRepository.GetMostRecentByUserAsync(userId);

        if (attendance == null)
        {
            logger.LogWarning($"Most recent attendance for user with ID {userId} was not found");
            return MethodResponse<AttendanceDto>.Failure(
                new Error(ErrorConstants.AttendanceNotFound, "Attendances were not found")
            );
        }
        
        var attendanceDto = new AttendanceDto(attendance);
        var serializedAttendance = JsonSerializer.Serialize(attendanceDto);
        await cacheRepository.SetAsync(Constants.RecentAttendancePrefix + 
                                            Constants.UserPrefix + userId, serializedAttendance,Constants.ShortCachePeriod);
        
        logger.LogInformation($"Successfully retrieved most recent attendance by user with ID {userId}");
        return MethodResponse<AttendanceDto>.Success(attendanceDto);
    }
    
    
    public async Task<MethodResponse<bool>> AddAttendanceAsync(AttendanceRequest request, string email, string clientApp)
    {
        var failureCount = 0;
        foreach (var date in request.AttendanceDates)
        {
            var newAttendance = new AttendanceEntity()
            {
                CourseId = request.CourseId,
                ClassroomId =  request.ClassroomId,
                TypeId = request.AttendanceTypeId,
                StartTime = date.ToDateTime(request.StartTime).ToUniversalTime(),
                EndTime = date.ToDateTime(request.EndTime).ToUniversalTime(),
                AutomatedRegistration =  request.AutomatedRegistration,
                CreatedBy = email,
                CreatedByClient =  clientApp,
                UpdatedBy = email,
                UpdatedByClient =  clientApp,
            };
            
            if (await attendanceRepository.CreateAsync(newAttendance) == null)
            {
                logger.LogWarning($"Attendance with date {date} was not added");
                failureCount++;
            }
        }

        if (failureCount > 0)
        {
            logger.LogWarning($"{failureCount} attendances were not added");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AttendanceNotCreated, "Some attendances were not created")
            );
        }
        
        logger.LogInformation($"Successfully created {request.AttendanceDates.Count} attendances");
        return MethodResponse<bool>.Success(true);
    }

    public async Task<MethodResponse<bool>> EditAttendanceAsync(Guid id, AttendanceChangeRequest request, 
                                                                                        string email, string clientApp)
    {
        var updatedAttendance = new AttendanceEntity()
        {
            ClassroomId = request.ClassroomId,
            TypeId = request.AttendanceTypeId,
            StartTime = request.StartTime.ToUniversalTime(),
            EndTime = request.EndTime.ToUniversalTime(),
            AutomatedRegistration =  request.AutomatedRegistration,
            UpdatedBy = email,
            UpdatedByClient = clientApp
        };
        
        await cacheRepository.DeletePatternAsync($"*{id.ToString()}*");
        var status = await attendanceRepository.UpdateAsync(updatedAttendance);

        if (status == null)
        {
            logger.LogWarning($"Updating attendance with ID {id} failed");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AttendanceNotUpdated, "Attendance was not updated")
            );
        }

        logger.LogInformation($"Successfully updated attendance with ID {id}");
        return MethodResponse<bool>.Success(true);
    }

    public async Task<MethodResponse<bool>> SoftDeleteAttendanceAsync(Guid id, string email, string clientApp)
    {
        var attendance = await attendanceRepository.GetByIdAsync(id);
        if (attendance == null)
        {
            logger.LogWarning($"Deleting attendance with ID {id} failed");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AttendanceNotFound, "Attendance was not found")
            );
        }
        
        await cacheRepository.DeletePatternAsync($"*{id.ToString()}*");
        var status = await attendanceRepository.ToggleDeletionAsync(id, email, clientApp, true);
        
        if (!status)
        {
            logger.LogWarning($"Deleting attendance with ID {id} failed");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AttendanceNotDeleted, "Attendance was not deleted")
            );
        }
        
        logger.LogInformation($"Successfully deleted attendance with ID {id}");
        return MethodResponse<bool>.Success(true);
    }
}