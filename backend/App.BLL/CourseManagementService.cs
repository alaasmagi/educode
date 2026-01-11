using System.Text.Json;
using App.DAL.EF;
using App.Domain;
using App.DTO;
using Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;


namespace App.BLL;

public class CourseManagementService : ICourseManagementService
{
    private readonly CourseRepository _courseRepository;
    private readonly AttendanceRepository _attendanceRepository;
    private readonly RedisRepository _redisRepository;
    private readonly UserRepository _userRepository;
    private readonly ILogger<CourseManagementService> _logger;

    public CourseManagementService(AppDbContext context, ILogger<CourseManagementService> logger, IConnectionMultiplexer connectionMultiplexer, ILogger<RedisRepository> redisLogger)
    {
        _logger = logger;
        _courseRepository = new CourseRepository(context);
        _attendanceRepository = new AttendanceRepository(context);
        _redisRepository = new RedisRepository(connectionMultiplexer, redisLogger); 
        _userRepository = new UserRepository(context);
    }

    public async Task<CourseEntity?> GetCourseByAttendanceIdAsync(Guid attendanceId)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.CoursePrefix + 
                                                        Constants.AttendancePrefix + attendanceId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<CourseEntity>(cache);
        }
        
        var courseAttendance = await _attendanceRepository.GetAttendanceById(attendanceId);

        if (courseAttendance == null)
        {
            _logger.LogError($"Course attendance with id {attendanceId} not found");
            return null;
        }
        
        var course = await _courseRepository.GetCourseById(courseAttendance.CourseId);

        if (course == null)
        {
            _logger.LogError($"Course with attendance with ID {attendanceId} was not found");
            return null;
        }
        
        var serializedCourse = JsonSerializer.Serialize(course);
        await _redisRepository.SetDataAsync(Constants.CoursePrefix + Constants.AttendancePrefix + attendanceId, 
                                                                                    serializedCourse, Constants.LongCachePeriod);
        return course;
    }

    public async Task<CourseEntity?> GetCourseByIdAsync(Guid courseId, string email)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.CoursePrefix + courseId);
        CourseEntity? course;

        if (cache != null)
        {
            course = JsonSerializer.Deserialize<CourseEntity>(cache);
        }
        else
        {
            course = await _courseRepository.GetCourseById(courseId);

            if (course == null)
            {
                _logger.LogError($"Course with ID {courseId} was not found");
                return null;
            }

            var serializedCourse = JsonSerializer.Serialize(course);
            await _redisRepository.SetDataAsync(Constants.CoursePrefix + courseId, serializedCourse, Constants.MediumCachePeriod);
        }

        var accessible = await IsCourseAccessibleToUser(course, email);
        if (!accessible)
        {
            _logger.LogError($"Course with ID {course.Id} cannot be fetched by user {email}");
            return null;
        }

        return course;
    }
    
    public async Task<bool> AddCourse(UserEntity user, CourseEntity course, string creator)
    {
        var courseExists = await DoesCourseExistByCodeAsync(course.CourseCode);

        if (courseExists)
        {
            _logger.LogError($"Course with code {course.CourseCode} already exists");
            return false;
        }
        
        var courseTeacher = new CourseTeacherEntity
        {
            TeacherId = user.Id,
            CreatedBy = creator,
            UpdatedBy = creator
        };
        
        var status = await _courseRepository.AddCourseEntity(courseTeacher, course);

        if (!status)
        {
            _logger.LogError("Failed to add course");
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> EditCourse(Guid courseId, CourseEntity newCourse)
    {
        var courseExistence = await DoesCourseExistByIdAsync(courseId);
        
        if (!courseExistence)
        {
            _logger.LogError($"Failed to update course with id {courseId}");
            return false;
        }
        
        await _redisRepository.DeleteKeysByPatternAsync($"*{courseId.ToString()}*");
        var status = await _courseRepository.UpdateCourseEntity(courseId, newCourse);
        if (!status)
        {
            _logger.LogError($"Failed to update course with id {courseId}");
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> DeleteCourse(Guid courseId, string email)
    {
        var course = await GetCourseByIdAsync(courseId, email);
        
        if (course == null)
        {
            _logger.LogError($"Failed to delete course with id {courseId}");
            return false;
        }
        
        await _redisRepository.DeleteKeysByPatternAsync($"*{courseId.ToString()}*");
        
        var status = await _courseRepository.DeleteCourseEntity(course);
        
        if (!status)
        {
            _logger.LogError($"Failed to delete course with id {courseId}");
            return false;
        }
        
        return true;
    }
    
    public async Task<List<CourseStatusEntity>?> GetAllCourseStatuses()
    {
        var cache = await _redisRepository.GetDataAsync(Constants.CourseStatusPrefix);
        
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<CourseStatusEntity>?>(cache);
        }
        
        var courseStatuses = await _courseRepository.GetAllCourseStatuses();
        
        if (courseStatuses != null && courseStatuses.Count <= 0)
        {
            _logger.LogError($"Failed to get course statuses");
            return null;
        }
        
        var serializedCourseStatuses = JsonSerializer.Serialize(courseStatuses);
        await _redisRepository.SetDataAsync(Constants.CourseStatusPrefix, 
            serializedCourseStatuses, Constants.ExtraLongCachePeriod);
        
        return courseStatuses;
    }
    
    public async Task<List<CourseEntity>?> GetCoursesByUserAsync(Guid userId, int pageNr, int pageSize)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.CoursePrefix + 
                                                        Constants.UserPrefix + userId + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<CourseEntity>?>(cache);
        }
        
        var coursesByUser = await _courseRepository.GetCoursesByUser(userId, pageNr, pageSize);
        if (coursesByUser == null)
        {
            _logger.LogError($"Failed to get courses by user with ID {userId}");
            return null;
        }

        var serializedCoursesByUser = JsonSerializer.Serialize(coursesByUser);
        await _redisRepository.SetDataAsync(Constants.CoursePrefix + Constants.UserPrefix + userId + pageNr + pageSize, 
            serializedCoursesByUser, Constants.ShortCachePeriod);
        
        return coursesByUser;
    }
    
    public async Task<List<AttendanceStudentCountDto>?> GetAttendancesUserCountsByCourseAsync(Guid courseId)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.CourseStudentCountsPrefix + courseId);

        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceStudentCountDto>>(cache);
        }
        
        var studentCounts = await _courseRepository.GetAllUserCountsByCourseId(courseId);
        if (studentCounts == null)
        {
            _logger.LogError($"Failed to get attendances user counts by course with ID {courseId}");
            return null;
        }
        
        var serializedStudentCounts = JsonSerializer.Serialize(studentCounts);
        await _redisRepository.SetDataAsync(Constants.CourseStudentCountsPrefix + courseId, 
            serializedStudentCounts, Constants.ShortCachePeriod);
        return studentCounts;
    }
    
    public async Task<bool> IsCourseAccessibleToUser(CourseEntity courseEntity, string email)
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
        
        var accessCache = await _redisRepository.GetDataAsync(Constants.CourseAccessPrefix + courseEntity.Id + user.Id);
        int access;

        if (accessCache != null)
        {
            access = int.Parse(accessCache);
        }
        else
        {
            access = await _courseRepository.CourseAccessibilityCheck(courseEntity.Id, user.Id);
            await _redisRepository.SetDataAsync(Constants.CourseAccessPrefix + courseEntity.Id + user.Id, access.ToString(), 
                Constants.ShortCachePeriod);
        }

        if (access <= 0)
        {
            _logger.LogError($"Course with ID {courseEntity.Id} is not accessible by user with email {email}");
            return false;
        }

        return true;
    }
    
    public async Task<bool> DoesCourseExistByCodeAsync(string courseCode)
    {
        var status = await _courseRepository.CourseAvailabilityCheckByCourseCode(courseCode);

        if (!status)
        {
            _logger.LogError($"Course with code {courseCode} was not found");
            return false;
        }
        
        _logger.LogInformation($"Course with code {courseCode} was found");
        return true;        
    }
    
    public async Task<bool> DoesCourseExistByIdAsync(Guid id)
    {
        var status = await _courseRepository.CourseAvailabilityCheckById(id);

        if (!status)
        {
            _logger.LogError($"Course with code {id} was not found");
            return false;
        }
        
        _logger.LogInformation($"Course with code {id} was found");
        return true;        
    }
    
    // TODO: Implement soft deletion that cascade-soft-deletes CourseTeachers, CourseAttendances

    // TODO: Implement a method which can search and authenticate Courses that are soft deleted (IgnoreQueryFilters())
    
    // TODO: Implement restoration method that cascade-restores CourseTeachers, CourseAttendances
    
    // TODO: Implement an approach that can add multiple Teachers to one course
    
    // TODO: Implement an approach that can remove teachers
}
   
    