using System.Text.Json;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace App.BLL;

public class AttendanceManagementService : IAttendanceManagementService
{
    private readonly ILogger<AttendanceManagementService> _logger;
    private readonly AttendanceRepository _attendanceRepository;
    private readonly CourseRepository _courseRepository;
    private readonly UserRepository _userRepository;
    private readonly RedisRepository _redisRepository;

    public AttendanceManagementService(AppDbContext context, ILogger<AttendanceManagementService> logger,
                                    IConnectionMultiplexer connectionMultiplexer, ILogger<RedisRepository> redisLogger)
    {
        _logger = logger;
        _attendanceRepository = new AttendanceRepository(context);
        _redisRepository = new RedisRepository(connectionMultiplexer, redisLogger); 
        _courseRepository = new CourseRepository(context); 
        _userRepository = new UserRepository(context); 
    }
    public async Task<bool> DoesWorkplaceExist(string workplaceIdentifier)
    {
        var result = await _attendanceRepository.WorkplaceAvailabilityCheckById(workplaceIdentifier);
        if (!result)
        {
            _logger.LogError($"Workplace with id {workplaceIdentifier} was not found");
            return false;
        }

        return true;
    }
    
    public async Task<bool> DoesAttendanceExist(Guid attendanceId)
    {
        var result = await _attendanceRepository.AttendanceAvailabilityCheckById(attendanceId);

        if (!result)
        {
            _logger.LogError($"Attendance with ID {attendanceId} was not found");
            return false;
        }
        
        _logger.LogInformation($"Attendance with ID {attendanceId} was found");
        return true;
    }
    
    public async Task<bool> DoesAttendanceCheckExist(string studentCode, string fullName, string attendanceIdentifier)
    {
        var result = await _attendanceRepository.AttendanceCheckAvailabilityCheck(studentCode, attendanceIdentifier);

        if (!result)
        {
            _logger.LogError($"AttendanceCheck with student code {studentCode}, fullname {fullName} and attendance identifier {attendanceIdentifier} was not found");
            return false;
        }
        
        _logger.LogInformation($"AttendanceCheck with student code {studentCode} and attendance identifier {attendanceIdentifier} was found");
        return true;
    }
    
    public async Task<CourseAttendanceEntity?> GetCurrentAttendanceAsync(Guid userId)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.CurrentAttendancePrefix + 
                                                                                        Constants.UserPrefix + userId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<CourseAttendanceEntity?>(cache);
        }
        
        var currentAttendance = await _attendanceRepository.GetCurrentAttendance(userId);
        if (currentAttendance == null)
        {
            _logger.LogError($"Current attendance for user with ID {userId} was not found");
            return null;
        }
        
        var serializedAttendance = JsonSerializer.Serialize(currentAttendance);
        await _redisRepository.SetDataAsync(Constants.CurrentAttendancePrefix + 
                                            Constants.UserPrefix + userId, serializedAttendance,Constants.ExtraShortCachePeriod);
        
        return currentAttendance;
    }
    
    public async Task<CourseAttendanceEntity?> GetCourseAttendanceByIdAsync(Guid attendanceId, string email)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendancePrefix + attendanceId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<CourseAttendanceEntity?>(cache);
        }
        
        var courseAttendance = await _attendanceRepository.GetAttendanceById(attendanceId);

        if (courseAttendance == null)
        {
            _logger.LogError($"Attendance with ID {attendanceId} was not found");
            return null;
        }
        
        var serializedAttendance = JsonSerializer.Serialize(courseAttendance);
        await _redisRepository.SetDataAsync(Constants.AttendancePrefix + attendanceId, serializedAttendance,Constants.MediumCachePeriod);
        
        var accessible = await IsAttendanceAccessibleByUser(courseAttendance, email);
        if (!accessible)
        {
            _logger.LogError($"AttendanceCheck with ID {attendanceId} cannot be fetched");
            return null;
        }
        return courseAttendance;
    }
    
    public async Task<CourseAttendanceEntity?> GetCourseAttendanceByIdentifier(string identifier)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendancePrefix + identifier);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<CourseAttendanceEntity?>(cache);
        }
        
        var courseAttendance = await _attendanceRepository.GetAttendanceByIdentifier(identifier);

        if (courseAttendance == null)
        {
            _logger.LogError($"Attendance with identifier {identifier} was not found");
            return null;
        }
        
        var serializedAttendance = JsonSerializer.Serialize(courseAttendance);
        await _redisRepository.SetDataAsync(Constants.AttendancePrefix + identifier, serializedAttendance,Constants.MediumCachePeriod);
        
        return courseAttendance;
    }

    public async Task<int> GetStudentsCountByAttendanceIdAsync(string attendanceIdentifier)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendancePrefix + Constants.StudentCountPrefix + attendanceIdentifier);
        if (cache != null)
        {
            return int.Parse(cache);
        }
        
        var result = await _attendanceRepository.GetStudentCountByAttendanceId(attendanceIdentifier);
        
        await _redisRepository.SetDataAsync(Constants.AttendancePrefix + Constants.StudentCountPrefix + attendanceIdentifier, result.ToString(), 
            Constants.ShortCachePeriod);
        
        if (result <= 0)
        {
            _logger.LogError($"Attendance with identifier {attendanceIdentifier} has no attendance checks");
            return 0;
        }
        return result;
    }
    
    public async Task<List<CourseAttendanceEntity>?> GetAttendancesByCourseAsync(Guid courseId, int pageNr, int pageSize)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendancePrefix + Constants.CoursePrefix + courseId + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<CourseAttendanceEntity>?>(cache);
        }
        
        var attendances = await _attendanceRepository.GetCourseAttendancesByCourseId(courseId, pageNr, pageSize);

        if (attendances.Count <= 0)
        {
            _logger.LogError($"Attendances by course with ID {courseId} were not found");
            return null;
        }

        var serializedAttendancesByCourse = JsonSerializer.Serialize(attendances);
        await _redisRepository.SetDataAsync(Constants.AttendancePrefix + Constants.CoursePrefix + courseId + pageNr + pageSize, 
            serializedAttendancesByCourse, Constants.ShortCachePeriod);
        
        return attendances;
    }

    public async Task<bool> AddAttendanceCheckAsync(AttendanceCheckEntity attendanceCheck, string creator, string? workplaceIdentifer)
    {
        var attendanceCheckExist = await DoesAttendanceCheckExist(attendanceCheck.StudentCode, attendanceCheck.FullName, attendanceCheck.AttendanceIdentifier);

        if (attendanceCheckExist)
        {
            _logger.LogError($"Attendance check adding failed");
            return false;
        }
        
        bool status;
        attendanceCheck.StudentCode = attendanceCheck.StudentCode.ToUpper();
        if (workplaceIdentifer != null)
        {
            var workplace = await _attendanceRepository.GetWorkplaceByIdentifier(workplaceIdentifer);
            status = await _attendanceRepository.AddAttendanceCheck(attendanceCheck, creator, workplace);
        }
        else
        {
            status = await _attendanceRepository.AddAttendanceCheck(attendanceCheck, creator, null);
        }
        
        if (!status)
        {
            _logger.LogError($"Attendance check adding failed");
            return false;
        }
        
        return true;
    }

    public async Task<List<AttendanceCheckEntity>?> GetAttendanceChecksByAttendanceIdAsync(string attendanceIdentifier, 
                                                                                                int pageNr, int pageSize)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + attendanceIdentifier + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceCheckEntity>?>(cache);
        }
        
        var attendanceChecks = await _attendanceRepository.GetAttendanceChecksByAttendanceIdentifier(attendanceIdentifier, pageNr, pageSize);
        
        if (attendanceChecks.Count <= 0)
        {
            _logger.LogError($"Attendance checks for attendance with identifier {attendanceIdentifier} were not found");
            return null;
        }
        
        var serializedAttendanceChecksByAttendance = JsonSerializer.Serialize(attendanceChecks);
        await _redisRepository.SetDataAsync(Constants.AttendanceCheckPrefix + Constants.AttendancePrefix + attendanceIdentifier + pageNr + pageSize, 
            serializedAttendanceChecksByAttendance, Constants.ShortCachePeriod);
        
        return attendanceChecks;
    }

    public async Task<CourseAttendanceEntity?> GetMostRecentAttendanceByUserAsync(Guid userId)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.RecentAttendancePrefix + 
                                                        Constants.UserPrefix + userId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<CourseAttendanceEntity?>(cache);
        }
        
        var attendance = await _attendanceRepository.GetMostRecentAttendanceByUser(userId);

        if (attendance == null)
        {
            _logger.LogError($"Most recent attendance for user with ID {userId} was not found");
            return null;
        }
        
        var serializedAttendance = JsonSerializer.Serialize(attendance);
        await _redisRepository.SetDataAsync(Constants.RecentAttendancePrefix + 
                                            Constants.UserPrefix + userId, serializedAttendance,Constants.ShortCachePeriod);
        
        return attendance;
    }

    public async Task<AttendanceCheckEntity?> GetAttendanceCheckByIdAsync(Guid attendanceCheckId, string email)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendanceCheckPrefix + attendanceCheckId);
        AttendanceCheckEntity? attendanceCheck;

        if (cache != null)
        {
            attendanceCheck = JsonSerializer.Deserialize<AttendanceCheckEntity>(cache);
        }
        else
        {
            attendanceCheck = await _attendanceRepository.GetAttendanceCheckById(attendanceCheckId);

            if (attendanceCheck == null)
            {
                _logger.LogError($"AttendanceCheck with ID {attendanceCheck} was not found");
                return null;
            }

            var serializedAttendance = JsonSerializer.Serialize(attendanceCheck);
            await _redisRepository.SetDataAsync(Constants.AttendanceCheckPrefix + attendanceCheckId, serializedAttendance, Constants.MediumCachePeriod);
        }
        
        var accessible = await IsAttendanceCheckAccessibleByUser(attendanceCheck, email);
        if (!accessible)
        {
            _logger.LogError($"AttendanceCheck with ID {attendanceCheckId} cannot be fetched");
            return null;
        }
        
        return attendanceCheck;
    }
    
    public async Task<List<AttendanceTypeEntity>?> GetAttendanceTypesAsync()
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendanceTypePrefix);
        
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceTypeEntity>?>(cache);
        }
        
        var result = await _attendanceRepository.GetAttendanceTypes();
        if (result.Count <= 0)
        {
            _logger.LogError($"Failed to get course statuses");
            return null;
        }
        
        var serializedAttendanceTypes = JsonSerializer.Serialize(result);
        await _redisRepository.SetDataAsync(Constants.AttendanceTypePrefix, 
            serializedAttendanceTypes, Constants.ExtraLongCachePeriod);
        
        return result;
    }

    public async Task<AttendanceTypeEntity?> GetAttendanceTypeByIdAsync(Guid attendanceTypeId)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.AttendanceTypePrefix + attendanceTypeId);
        
        if (cache != null)
        {
            return JsonSerializer.Deserialize<AttendanceTypeEntity?>(cache);
        }
        
        var result = await _attendanceRepository.GetAttendanceTypeById(attendanceTypeId);
        
        if (result == null)
        {
            _logger.LogError($"Attendance type with ID {attendanceTypeId} was not found");
            return null;
        }
        
        var serializedAttendanceType = JsonSerializer.Serialize(result);
        await _redisRepository.SetDataAsync(Constants.AttendanceTypePrefix + attendanceTypeId, 
            serializedAttendanceType, Constants.ExtraLongCachePeriod);
        
        return result;
    }
    
    public async Task<bool> AddAttendanceAsync(CourseAttendanceEntity attendance, List<DateOnly> attendanceDates, 
                                                                                TimeOnly startTime, TimeOnly endTime)
    {
        var failureCount = 0;
        foreach (var date in attendanceDates)
        {
            var newAttendance = new CourseAttendanceEntity()
            {
                CourseId = attendance.CourseId,
                AttendanceTypeId = attendance.AttendanceTypeId,
                AttendanceType = attendance.AttendanceType,
                StartTime = date.ToDateTime(startTime),
                EndTime = date.ToDateTime(endTime),
                CreatedBy = attendance.CreatedBy,
                UpdatedBy = attendance.UpdatedBy
            };
            
            if (!await _attendanceRepository.AddAttendance(newAttendance))
            {
                _logger.LogError($"Attendance with date {date} was not added");
                failureCount++;
            }
        }

        if (failureCount > 0)
        {
            _logger.LogError($"{failureCount} attendances were not added");
            return false;
        }
        return true;
    }

    public async Task<bool> EditAttendanceAsync(Guid attendanceId, CourseAttendanceEntity updatedAttendance)
    {
        if (!await DoesAttendanceExist(attendanceId))
        {
            _logger.LogError($"Updating attendance with ID {attendanceId} failed");
            return false;
        }
        
        await _redisRepository.DeleteKeysByPatternAsync($"*{attendanceId.ToString()}*");
        var status = await _attendanceRepository.UpdateAttendance(attendanceId, updatedAttendance);

        if (!status)
        {
            _logger.LogError($"Updating attendance with ID {attendanceId} failed");
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAttendance(Guid attendanceId, string email)
    {
        var attendance = await GetCourseAttendanceByIdAsync(attendanceId, email);
        if (attendance == null)
        {
            _logger.LogError($"Deleting attendance with ID {attendanceId} failed");
            return false;
        }
        
        await _redisRepository.DeleteKeysByPatternAsync($"*{attendanceId.ToString()}*");
        var status = await _attendanceRepository.DeleteAttendanceEntity(attendance);
        
        if (!status)
        {
            _logger.LogError($"Deleting attendance with ID {attendanceId} failed");
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> DeleteAttendanceCheck(Guid attendanceCheckId, string email)
    {
        var attendanceCheck = await GetAttendanceCheckByIdAsync(attendanceCheckId, email);
        if (attendanceCheck == null)
        {
            _logger.LogError($"Deleting attendance check with ID {attendanceCheckId} failed");
            return false;
        }
        
        await _redisRepository.DeleteKeysByPatternAsync($"*{attendanceCheckId.ToString()}*");
        var status = await _attendanceRepository.DeleteAttendanceCheckEntity(attendanceCheck);

        if (!status)
        {
            _logger.LogError($"Deleting attendance check with ID {attendanceCheckId} failed");
            return false;
        }
        return true;
    }

    public async Task<bool> IsAttendanceAccessibleByUser(CourseAttendanceEntity attendance, string email)
    {
        var userCache = await _redisRepository.GetDataAsync(Constants.UserPrefix + email);
        UserEntity? user;
        
        if (userCache != null)
        {
            user = JsonSerializer.Deserialize<UserEntity?>(userCache);
        }
        else
        {
            user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                var serializedUser = JsonSerializer.Serialize(user);
                await _redisRepository.SetDataAsync(Constants.UserPrefix + email, serializedUser, 
                    Constants.DefaultCachePeriod);
            }
        }
        
        if (user == null)
        {
            _logger.LogError($"User with email {email} was not found");
            return false;
        }
        
        var accessCache = await _redisRepository.GetDataAsync(Constants.AttendanceAccessPrefix + attendance.Id + user.Id);
        int access;

        if (accessCache != null)
        {
            access = int.Parse(accessCache);
        }
        else
        {
            access = await _courseRepository.CourseAccessibilityCheck(attendance.CourseId, user.Id);
            await _redisRepository.SetDataAsync(Constants.AttendanceAccessPrefix + attendance.Id + user.Id, access.ToString(), 
                Constants.ShortCachePeriod);
        }

        if (access <= 0)
        {
            _logger.LogError($"Attendance with ID {attendance.Id} is not accessible by user with email {email}");
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> IsAttendanceCheckAccessibleByUser(AttendanceCheckEntity attendanceCheck, string email)
    {
        
        var userCache = await _redisRepository.GetDataAsync(Constants.UserPrefix + email);
        UserEntity? user;
        
        if (userCache != null)
        {
            user = JsonSerializer.Deserialize<UserEntity?>(userCache);
        }
        else
        {
            user = await _userRepository.GetUserByEmailAsync(email);
            if (user != null)
            {
                var serializedUser = JsonSerializer.Serialize(user);
                await _redisRepository.SetDataAsync(Constants.UserPrefix + email, serializedUser, 
                    Constants.DefaultCachePeriod);
            }
        }
        
        if (user == null)
        {
            _logger.LogError($"User with email {email} was not found");
            return false;
        }
        
        var attendance = await GetCourseAttendanceByIdentifier(attendanceCheck.AttendanceIdentifier);
        if (attendance == null)
        {
            _logger.LogError($"Attendance with identifier {attendanceCheck.AttendanceIdentifier} was not found");
            return false;
        }
        
        var accessCache = await _redisRepository.GetDataAsync(Constants.AttendanceAccessPrefix + attendance.Id + user.Id);
        int access;

        if (accessCache != null)
        {
            access = int.Parse(accessCache);
        }
        else
        {
            access = await _courseRepository.CourseAccessibilityCheck(attendance.CourseId, user.Id);
            await _redisRepository.SetDataAsync(Constants.AttendanceAccessPrefix + attendance.Id + user.Id, access.ToString(), 
                Constants.ShortCachePeriod);
        }

        if (access <= 0)
        {
            _logger.LogError($"Attendance check with ID {attendanceCheck.Id} is not accessible by user with email {email}");
            return false;
        }
        
        return true;
    }
}