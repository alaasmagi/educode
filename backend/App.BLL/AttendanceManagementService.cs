using System.Text.Json;
using App.BLL.Contracts;
using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.Extensions.Logging;

namespace App.BLL;

public class AttendanceManagementService(
    ICacheRepository cacheRepository, 
    IUserRepository userRepository,
    IAttendanceCheckRepository attendanceCheckRepository,
    IAttendanceTypeRepository attendanceTypeRepository,
    IWorkplaceRepository workplaceRepository,
    IAttendanceRepository attendanceRepository,
    ILogger<AttendanceManagementService> logger) : IAttendanceManagementService
{
    public async Task<AttendanceEntity?> GetCurrentAttendanceAsync(Guid userId)
    {
        var cache = await cacheRepository.GetAsync(Constants.CurrentAttendancePrefix + 
                                                                                        Constants.UserPrefix + userId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<AttendanceEntity?>(cache);
        }
        
        var currentAttendance = await attendanceRepository.GetOngoingByUserAsync(userId);
        if (currentAttendance == null)
        {
            logger.LogError($"Current attendance for user with ID {userId} was not found");
            return null;
        }
        
        var serializedAttendance = JsonSerializer.Serialize(currentAttendance);
        await cacheRepository.SetAsync(Constants.CurrentAttendancePrefix + 
                                            Constants.UserPrefix + userId, serializedAttendance,Constants.ExtraShortCachePeriod);
        
        return currentAttendance;
    }
    
    public async Task<AttendanceEntity?> GetCourseAttendanceByIdAsync(Guid attendanceId, string email)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendancePrefix + attendanceId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<AttendanceEntity?>(cache);
        }
        
        var courseAttendance = await attendanceRepository.GetByIdAsync(attendanceId);

        if (courseAttendance == null)
        {
            logger.LogError($"Attendance with ID {attendanceId} was not found");
            return null;
        }
        
        var serializedAttendance = JsonSerializer.Serialize(courseAttendance);
        await cacheRepository.SetAsync(Constants.AttendancePrefix + attendanceId, serializedAttendance,Constants.MediumCachePeriod);
        
        var accessible = await IsAttendanceAccessibleByUser(courseAttendance, email);
        if (!accessible)
        {
            logger.LogError($"AttendanceCheck with ID {attendanceId} cannot be fetched");
            return null;
        }
        return courseAttendance;
    }

    public async Task<int> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendancePrefix + Constants.StudentCountPrefix + attendanceIdentifier);
        if (cache != null)
        {
            return int.Parse(cache);
        }
        
        var attendanceId = await attendanceRepository.CheckAvailabilityByIdentifierAsync(attendanceIdentifier);
        
        if(attendanceId == null)
        {
            logger.LogError($"Attendance with identifier {attendanceIdentifier} was not found");
            return 0;
        }
        
        var result = await attendanceCheckRepository.GetUserCountAsync(attendanceId.Value);
        
        if (result == null)
        {
            logger.LogError($"Attendance with identifier {attendanceIdentifier} has no attendance checks");
            return 0;
        }
        
        await cacheRepository.SetAsync(Constants.AttendancePrefix + Constants.StudentCountPrefix + attendanceIdentifier, result.ToString(), 
            Constants.ShortCachePeriod);
        
        return result.Value;
    }
    
    public async Task<List<AttendanceEntity>?> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendancePrefix + Constants.CoursePrefix + courseId + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceEntity>?>(cache);
        }
        
        var attendances = await attendanceRepository.GetAllByCourseAsync(courseId, pageNr, pageSize);

        if (attendances == null)
        {
            logger.LogError($"Attendances by course with ID {courseId} were not found");
            return null;
        }

        var serializedAttendancesByCourse = JsonSerializer.Serialize(attendances);
        await cacheRepository.SetAsync(Constants.AttendancePrefix + Constants.CoursePrefix + courseId + pageNr + pageSize, 
            serializedAttendancesByCourse, Constants.ShortCachePeriod);
        
        return attendances;
    }

    public async Task<bool> AddAttendanceCheckAsync(AttendanceCheckEntity attendanceCheck, string creator, string? workplaceIdentifer)
    {
        AttendanceCheckEntity? status;
        attendanceCheck.StudentCode = attendanceCheck.StudentCode.ToUpper();
        if (workplaceIdentifer != null)
        {
            var workplaceId = await workplaceRepository.CheckAvailabilityByIdentifierAsync(workplaceIdentifer);
            
            if (workplaceId == null)
            {
                logger.LogError($"Workplace with identifier {workplaceIdentifer} was not found");
                return false;
            }
            
            var workplace = await workplaceRepository.GetByIdAsync(workplaceId.Value);
            status = await attendanceCheckRepository.CreateAsync(attendanceCheck);
        }
        else
        {
            status = await attendanceCheckRepository.CreateAsync(attendanceCheck);
        }
        
        if (status == null)
        {
            logger.LogError($"Attendance check adding failed");
            return false;
        }
        
        return true;
    }

    public async Task<List<AttendanceCheckEntity>?> GetAttendanceChecksByAttendanceIdAsync(string attendanceIdentifier, 
                                                                                                int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + attendanceIdentifier + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceCheckEntity>?>(cache);
        }
        
        var attendanceId = await attendanceRepository.CheckAvailabilityByIdentifierAsync(attendanceIdentifier);
        
        if (attendanceId == null)
        {
            logger.LogError($"Attendance with identifier {attendanceIdentifier} was not found");
            return null;
        }
        
        var attendanceChecks = await attendanceCheckRepository.GetAllByAttendanceAsync(attendanceId.Value);
        
        if (attendanceChecks == null)
        {
            logger.LogError($"Attendance checks for attendance with identifier {attendanceIdentifier} were not found");
            return null;
        }
        
        var serializedAttendanceChecksByAttendance = JsonSerializer.Serialize(attendanceChecks);
        await cacheRepository.SetAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + attendanceIdentifier + pageNr + pageSize, 
            serializedAttendanceChecksByAttendance, Constants.ShortCachePeriod);
        
        return attendanceChecks;
    }

    public async Task<AttendanceEntity?> GetMostRecentAttendanceByUserAsync(Guid userId)
    {
        var cache = await cacheRepository.GetAsync(Constants.RecentAttendancePrefix + 
                                                        Constants.UserPrefix + userId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<AttendanceEntity?>(cache);
        }
        
        var attendance = await attendanceRepository.GetMostRecentByUserAsync(userId);

        if (attendance == null)
        {
            logger.LogError($"Most recent attendance for user with ID {userId} was not found");
            return null;
        }
        
        var serializedAttendance = JsonSerializer.Serialize(attendance);
        await cacheRepository.SetAsync(Constants.RecentAttendancePrefix + 
                                            Constants.UserPrefix + userId, serializedAttendance,Constants.ShortCachePeriod);
        
        return attendance;
    }

    public async Task<AttendanceCheckEntity?> GetAttendanceCheckByIdAsync(Guid attendanceCheckId, string email)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceCheckPrefix + attendanceCheckId);
        AttendanceCheckEntity? attendanceCheck;

        if (cache != null)
        {
            attendanceCheck = JsonSerializer.Deserialize<AttendanceCheckEntity>(cache);
        }
        else
        {
            attendanceCheck = await attendanceCheckRepository.GetByIdAsync(attendanceCheckId);

            if (attendanceCheck == null)
            {
                logger.LogError($"AttendanceCheck with ID {attendanceCheck} was not found");
                return null;
            }

            var serializedAttendance = JsonSerializer.Serialize(attendanceCheck);
            await cacheRepository.SetAsync(Constants.AttendanceCheckPrefix + attendanceCheckId, serializedAttendance, Constants.MediumCachePeriod);
        }
        
        return attendanceCheck;
    }
    
    public async Task<List<AttendanceTypeEntity>?> GetAttendanceTypesAsync()
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceTypePrefix);
        
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceTypeEntity>?>(cache);
        }
        
        var result = await attendanceTypeRepository.GetAllAsync(1, 100);
        if (result == null)
        {
            logger.LogError($"Failed to get course statuses");
            return null;
        }
        
        var serializedAttendanceTypes = JsonSerializer.Serialize(result);
        await cacheRepository.SetAsync(Constants.AttendanceTypePrefix, 
            serializedAttendanceTypes, Constants.ExtraLongCachePeriod);
        
        return result;
    }

    public async Task<AttendanceTypeEntity?> GetAttendanceTypeByIdAsync(Guid attendanceTypeId)
    {
        var cache = await cacheRepository.GetAsync(Constants.AttendanceTypePrefix + attendanceTypeId);
        
        if (cache != null)
        {
            return JsonSerializer.Deserialize<AttendanceTypeEntity?>(cache);
        }
        
        var result = await attendanceTypeRepository.GetByIdAsync(attendanceTypeId);
        
        if (result == null)
        {
            logger.LogError($"Attendance type with ID {attendanceTypeId} was not found");
            return null;
        }
        
        var serializedAttendanceType = JsonSerializer.Serialize(result);
        await cacheRepository.SetAsync(Constants.AttendanceTypePrefix + attendanceTypeId, 
            serializedAttendanceType, Constants.ExtraLongCachePeriod);
        
        return result;
    }
    
    public async Task<bool> AddAttendanceAsync(AttendanceEntity attendance, List<DateOnly> attendanceDates, 
                                                                                TimeOnly startTime, TimeOnly endTime)
    {
        var failureCount = 0;
        foreach (var date in attendanceDates)
        {
            var newAttendance = new AttendanceEntity()
            {
                CourseId = attendance.CourseId,
                AttendanceTypeId = attendance.AttendanceTypeId,
                AttendanceType = attendance.AttendanceType,
                StartTime = date.ToDateTime(startTime),
                EndTime = date.ToDateTime(endTime),
                CreatedBy = attendance.CreatedBy,
                UpdatedBy = attendance.UpdatedBy
            };
            
            if (await attendanceRepository.CreateAsync(newAttendance) == null)
            {
                logger.LogError($"Attendance with date {date} was not added");
                failureCount++;
            }
        }

        if (failureCount > 0)
        {
            logger.LogError($"{failureCount} attendances were not added");
            return false;
        }
        return true;
    }

    public async Task<bool> EditAttendanceAsync(Guid attendanceId, AttendanceEntity updatedAttendance)
    {
        await cacheRepository.DeletePatternAsync($"*{attendanceId.ToString()}*");
        var status = await attendanceRepository.UpdateAsync(updatedAttendance);

        if (status == null)
        {
            logger.LogError($"Updating attendance with ID {attendanceId} failed");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAttendance(Guid attendanceId, string email)
    {
        var attendance = await GetCourseAttendanceByIdAsync(attendanceId, email);
        if (attendance == null)
        {
            logger.LogError($"Deleting attendance with ID {attendanceId} failed");
            return false;
        }
        
        await cacheRepository.DeletePatternAsync($"*{attendanceId.ToString()}*");
        var status = await attendanceRepository.RemoveAsync(attendance);
        
        if (status == null)
        {
            logger.LogError($"Deleting attendance with ID {attendanceId} failed");
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> DeleteAttendanceCheck(Guid attendanceCheckId, string email)
    {
        var attendanceCheck = await GetAttendanceCheckByIdAsync(attendanceCheckId, email);
        if (attendanceCheck == null)
        {
            logger.LogError($"Deleting attendance check with ID {attendanceCheckId} failed");
            return false;
        }
        
        await cacheRepository.DeletePatternAsync($"*{attendanceCheckId.ToString()}*");
        var status = await attendanceCheckRepository.RemoveAsync(attendanceCheck);

        if (status == null)
        {
            logger.LogError($"Deleting attendance check with ID {attendanceCheckId} failed");
            return false;
        }
        return true;
    }

    public async Task<bool> IsAttendanceAccessibleByUser(AttendanceEntity attendance, string email)
    {
        var userCache = await cacheRepository.GetAsync(Constants.UserPrefix + email);
        UserEntity? user;
        
        if (userCache != null)
        {
            user = JsonSerializer.Deserialize<UserEntity?>(userCache);
        }
        else
        {
            var userId = await userRepository.CheckAvailabilityByEmailAsync(email);
            
            if (userId == null)
            {
                logger.LogError($"User with email {email} was not found");
                return false;
            }
            
            user = await userRepository.GetByIdAsync(userId.Value);
            if (user != null)
            {
                var serializedUser = JsonSerializer.Serialize(user);
                await cacheRepository.SetAsync(Constants.UserPrefix + email, serializedUser, 
                    Constants.DefaultCachePeriod);
            }
        }
        
        if (user == null)
        {
            logger.LogError($"User with email {email} was not found");
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> IsAttendanceCheckAccessibleByUser(AttendanceCheckEntity attendanceCheck, string email)
    {
        
        var userCache = await cacheRepository.GetAsync(Constants.UserPrefix + email);
        UserEntity? user;
        
        if (userCache != null)
        {
            user = JsonSerializer.Deserialize<UserEntity?>(userCache);
        }
        else
        {
            var userId = await userRepository.CheckAvailabilityByEmailAsync(email);
            
            if (userId == null)
            {
                logger.LogError($"Attendance with identifier {attendanceCheck.AttendanceIdentifier} was not found");
                return false;
            }
            
            user = await userRepository.GetByIdAsync(userId.Value);
            if (user != null)
            {
                var serializedUser = JsonSerializer.Serialize(user);
                await cacheRepository.SetAsync(Constants.UserPrefix + email, serializedUser, 
                    Constants.DefaultCachePeriod);
            }
        }
        
        if (user == null)
        {
            logger.LogError($"User with email {email} was not found");
            return false;
        }

        var attendanceId = await attendanceRepository.CheckAvailabilityByIdentifierAsync(attendanceCheck.AttendanceIdentifier);
        
        if (attendanceId == null)
        {
            logger.LogError($"Attendance with identifier {attendanceCheck.AttendanceIdentifier} was not found");
            return false;
        }
        
        var attendance = await attendanceRepository.GetByIdAsync(attendanceId.Value);
        if (attendance == null)
        {
            logger.LogError($"Attendance with identifier {attendanceCheck.AttendanceIdentifier} was not found");
            return false;
        }
        
        return true;
    }
    
    public void SeedAttendanceTypes()
    {
        var now = DateTime.UtcNow;

        var attendanceTypes = new List<AttendanceTypeEntity>
        {
            new AttendanceTypeEntity
            {
                AttendanceType = "lecture",
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new AttendanceTypeEntity
            {
                AttendanceType = "practice",
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new AttendanceTypeEntity
            {
                AttendanceType = "lecture-practice",
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            }
        };

        attendanceRepository.SeedAttendanceTypes(attendanceTypes);
    }
}