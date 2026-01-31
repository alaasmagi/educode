using System.Text.Json;
using App.BLL.Contracts;
using App.Common;
using App.DAL.Contracts;
using App.Domain;
using App.DTO;
using Microsoft.Extensions.Logging;


namespace App.BLL;

public class CourseManagementService (
    ICacheRepository cacheRepository, 
    ICourseRepository courseRepository,
    ICourseStatusRepository courseStatusRepository,
    ICourseTeacherRepository courseTeacherRepository,
    IAttendanceRepository attendanceRepository,
    ILogger<CourseManagementService> logger) : ICourseManagementService
{
    public async Task<CourseEntity?> GetCourseByAttendanceIdAsync(Guid attendanceId)
    {
        var cache = await cacheRepository.GetAsync(Constants.CoursePrefix + 
                                                   Constants.AttendancePrefix + attendanceId);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<CourseEntity>(cache);
        }
        
        var courseAttendance = await attendanceRepository.GetByIdAsync(attendanceId);

        if (courseAttendance == null)
        {
            logger.LogError($"Course attendance with id {attendanceId} not found");
            return null;
        }
        
        var course = await courseRepository.GetByIdAsync(courseAttendance.CourseId);

        if (course == null)
        {
            logger.LogError($"Course with attendance with ID {attendanceId} was not found");
            return null;
        }
        
        var serializedCourse = JsonSerializer.Serialize(course);
        await cacheRepository.SetAsync(Constants.CoursePrefix + Constants.AttendancePrefix + attendanceId, 
                                                                                    serializedCourse, Constants.LongCachePeriod);
        return course;
    }

    public async Task<CourseEntity?> GetCourseByIdAsync(Guid courseId, string email)
    {
        var cache = await cacheRepository.GetAsync(Constants.CoursePrefix + courseId);
        CourseEntity? course;

        if (cache != null)
        {
            course = JsonSerializer.Deserialize<CourseEntity>(cache);
        }
        else
        {
            course = await courseRepository.GetByIdAsync(courseId);

            if (course == null)
            {
                logger.LogError($"Course with ID {courseId} was not found");
                return null;
            }

            var serializedCourse = JsonSerializer.Serialize(course);
            await cacheRepository.SetAsync(Constants.CoursePrefix + courseId, serializedCourse, Constants.MediumCachePeriod);
        }
        
        return course;
    }
    
    public async Task<bool> AddCourse(UserEntity user, CourseEntity course, string creator)
    {
        var courseExists = await DoesCourseExistByCodeAsync(course.CourseCode);

        if (courseExists)
        {
            logger.LogError($"Course with code {course.CourseCode} already exists");
            return false;
        }
        
        var courseTeacher = new CourseTeacherEntity
        {
            TeacherId = user.Id,
            CreatedBy = creator,
            UpdatedBy = creator
        };
        
        
        ;
        if (await courseRepository.CreateAsync(course) == null || await courseTeacherRepository.CreateAsync(courseTeacher) == null)
        {
            logger.LogError("Failed to add course");
            return false;
        }
        return true;
    }
    
    public async Task<bool> EditCourse(Guid courseId, CourseEntity newCourse)
    {
        var courseExistence = await DoesCourseExistByIdAsync(courseId);
        
        if (!courseExistence)
        {
            logger.LogError($"Failed to update course with id {courseId}");
            return false;
        }
        
        await cacheRepository.DeletePatternAsync($"*{courseId.ToString()}*");
        var status = await courseRepository.UpdateAsync(newCourse);
        if (status == null)
        {
            logger.LogError($"Failed to update course with id {courseId}");
            return false;
        }
        
        return true;
    }
    
    public async Task<bool> DeleteCourse(Guid courseId, string email)
    {
        var course = await GetCourseByIdAsync(courseId, email);
        
        if (course == null)
        {
            logger.LogError($"Failed to delete course with id {courseId}");
            return false;
        }
        
        await cacheRepository.DeletePatternAsync($"*{courseId.ToString()}*");
        
        var status = await courseRepository.RemoveAsync(course);
        
        if (status == null)
        {
            logger.LogError($"Failed to delete course with id {courseId}");
            return false;
        }
        
        return true;
    }
    
    public async Task<List<CourseStatusEntity>?> GetAllCourseStatuses()
    {
        var cache = await cacheRepository.GetAsync(Constants.CourseStatusPrefix);
        
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<CourseStatusEntity>?>(cache);
        }
        
        var courseStatuses = await courseStatusRepository.GetAllAsync(1, 100);
        
        if (courseStatuses != null && courseStatuses.Count <= 0)
        {
            logger.LogError($"Failed to get course statuses");
            return null;
        }
        
        var serializedCourseStatuses = JsonSerializer.Serialize(courseStatuses);
        await cacheRepository.SetAsync(Constants.CourseStatusPrefix, 
            serializedCourseStatuses, Constants.ExtraLongCachePeriod);
        
        return courseStatuses;
    }
    
    public async Task<List<CourseEntity>?> GetCoursesByUserAsync(Guid userId, int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.CoursePrefix + 
                                                        Constants.UserPrefix + userId + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<CourseEntity>?>(cache);
        }
        
        var coursesByUser = await courseRepository.GetAllByUser(userId, pageNr, pageSize);
        if (coursesByUser == null)
        {
            logger.LogError($"Failed to get courses by user with ID {userId}");
            return null;
        }

        var serializedCoursesByUser = JsonSerializer.Serialize(coursesByUser);
        await cacheRepository.SetAsync(Constants.CoursePrefix + Constants.UserPrefix + userId + pageNr + pageSize, 
            serializedCoursesByUser, Constants.ShortCachePeriod);
        
        return coursesByUser;
    }
    
    public async Task<List<AttendanceStudentCountDto>?> GetAttendancesUserCountsByCourseAsync(Guid courseId)
    {
        var cache = await cacheRepository.GetAsync(Constants.CourseStudentCountsPrefix + courseId);

        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceStudentCountDto>>(cache);
        }
        
        var studentCounts = await courseRepository.GetUserCounts(courseId);
        if (studentCounts == null)
        {
            logger.LogError($"Failed to get attendances user counts by course with ID {courseId}");
            return null;
        }
        
        var serializedStudentCounts = JsonSerializer.Serialize(studentCounts);
        await cacheRepository.SetAsync(Constants.CourseStudentCountsPrefix + courseId, 
            serializedStudentCounts, Constants.ShortCachePeriod);
        return studentCounts;
    }
    
    public async Task<bool> DoesCourseExistByCodeAsync(string courseCode)
    {
        var status = await courseRepository.CheckAvailabilityByCodeAsync(courseCode);

        if (status == null)
        {
            logger.LogError($"Course with code {courseCode} was not found");
            return false;
        }
        
        logger.LogInformation($"Course with code {courseCode} was found");
        return true;        
    }
    
    public async Task<bool> DoesCourseExistByIdAsync(Guid id)
    {
        var status = await courseRepository.GetByIdAsync(id);

        if (status == null)
        {
            logger.LogError($"Course with code {id} was not found");
            return false;
        }
        
        logger.LogInformation($"Course with code {id} was found");
        return true;        
    }

    public void SeedCourseStatuses()
    {
        var now = DateTime.UtcNow;

        var courseStatuses = new List<CourseStatusEntity>
        {
            new CourseStatusEntity
            {
                CourseStatus = "available",
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new CourseStatusEntity
            {
                CourseStatus = "unavailable",
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new CourseStatusEntity
            {
                CourseStatus = "temp-unavailable",
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            }
        };
        courseRepository.SeedCourseStatuses(courseStatuses);
    }

    // TODO: Implement soft deletion that cascade-soft-deletes CourseTeachers, CourseAttendances

    // TODO: Implement a method which can search and authenticate Courses that are soft deleted (IgnoreQueryFilters())
    
    // TODO: Implement restoration method that cascade-restores CourseTeachers, CourseAttendances
    
    // TODO: Implement an approach that can add multiple Teachers to one course
    
    // TODO: Implement an approach that can remove teachers
}
   
    