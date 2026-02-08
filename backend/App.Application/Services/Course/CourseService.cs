using System.Text.Json;
using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.Course;

public class CourseService (
    ICacheRepository cacheRepository, 
    ICourseRepository courseRepository,
    ICourseStatusRepository courseStatusRepository,
    ICourseTeacherRepository courseTeacherRepository,
    IAttendanceRepository attendanceRepository,
    ILogger<CourseService> logger) : ICourseService
{
    public async Task<MethodResponse<CourseDto>> GetCourseByAttendanceIdAsync(Guid attendanceId)
    {
        var cache = await cacheRepository.GetAsync(Constants.CoursePrefix + 
                                                   Constants.AttendancePrefix + attendanceId);
        if (cache != null)
        {
            var deserializedCourse = JsonSerializer.Deserialize<CourseDto>(cache);
            return MethodResponse<CourseDto>.Success(deserializedCourse!);
        }
        
        var courseAttendance = await attendanceRepository.GetByIdAsync(attendanceId);

        if (courseAttendance == null)
        {
            logger.LogError($"Course attendance with id {attendanceId} not found");
            return MethodResponse<CourseDto>.Failure(
                new Error(ErrorConstants.AttendanceNotFound, "Attendance was not found")
            );
        }
        
        var course = await courseRepository.GetByIdAsync(courseAttendance.CourseId);

        if (course == null)
        {
            logger.LogError($"Course with attendance with ID {attendanceId} was not found");
            return MethodResponse<CourseDto>.Failure(
                new Error(ErrorConstants.CourseNotFound, "Course was not found")
            );
        }
        
        var courseDto = new CourseDto(course);
        var serializedCourseDto = JsonSerializer.Serialize(courseDto);
        await cacheRepository.SetAsync(Constants.CoursePrefix + Constants.AttendancePrefix + attendanceId, 
            serializedCourseDto, Constants.LongCachePeriod);
        
        logger.LogInformation($"Successfully retrieved course by attendance with ID {attendanceId}");
        return MethodResponse<CourseDto>.Success(courseDto);
    }

    public async Task<MethodResponse<CourseDto>> GetCourseByIdAsync(Guid courseId)
    {
        var cache = await cacheRepository.GetAsync(Constants.CoursePrefix + courseId);

        if (cache != null)
        {
            var deserializedCourseDto = JsonSerializer.Deserialize<CourseDto>(cache); 
            return MethodResponse<CourseDto>.Success(deserializedCourseDto!);
        }
        
        var course = await courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            logger.LogError($"Course with ID {courseId} was not found");
            return MethodResponse<CourseDto>.Failure(
                new Error(ErrorConstants.CourseNotFound, "Course was not found")
            );
        }

        var courseDto = new CourseDto(course);
        var serializedCourseDto = JsonSerializer.Serialize(courseDto);
        await cacheRepository.SetAsync(Constants.CoursePrefix + courseId, serializedCourseDto, Constants.MediumCachePeriod);
        
        
        logger.LogInformation($"Successfully retrieved course with ID {courseId}");
        return MethodResponse<CourseDto>.Success(courseDto);
    }
    
    public async Task<MethodResponse<bool>> AddCourse(UserEntity user, CourseEntity course, string creator)
    {
        var courseExists = await DoesCourseExistByCodeAsync(course.Code);

        if (courseExists)
        {
            logger.LogError($"Course with code {course.Code} already exists");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.CourseAlreadyExists, "Course already exists")
            );
        }
        
        var courseTeacher = new CourseTeacherEntity
        {
            TeacherId = user.Id,
            CourseId = course.Id,
            CreatedBy = creator,
            UpdatedBy = creator
        };
        
        if (await courseRepository.CreateAsync(course) == null || await courseTeacherRepository.CreateAsync(courseTeacher) == null)
        {
            logger.LogError("Failed to add course");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.CourseNotCreated, "Course was not created")
            );
        }
        
        logger.LogInformation($"Successfully added course with code {course.Code}");
        return MethodResponse<bool>.Success(true);
    }
    
    public async Task<MethodResponse<bool>> EditCourse(Guid courseId, CourseEntity newCourse, string client)
    {
        await cacheRepository.DeletePatternAsync($"*{courseId.ToString()}*");
        var status = await courseRepository.UpdateAsync(newCourse);
        if (status == null)
        {
            logger.LogError($"Failed to update course with id {courseId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.CourseNotUpdated, "Course was not updated")
            );
        }
        
        logger.LogInformation($"Successfully updated course with id {courseId}");
        return MethodResponse<bool>.Success(true);
    }
    
    public async Task<MethodResponse<bool>> DeleteCourse(Guid courseId, string email, string client)
    {
        var course = await courseRepository.GetByIdAsync(courseId);
        
        if (course == null)
        {
            logger.LogError($"Failed to delete course with id {courseId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.CourseNotFound, "Course was not found")
            );
        }
        
        await cacheRepository.DeletePatternAsync($"*{courseId.ToString()}*");
        
        var status = await courseRepository.RemoveAsync(course);
        
        if (status == null)
        {
            logger.LogError($"Failed to delete course with id {courseId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.CourseNotDeleted, "Course was not deleted")
            );
        }
        
        logger.LogInformation($"Successfully deleted course with ID {courseId}");
        return MethodResponse<bool>.Success(true);
    }
    
    public async Task<MethodResponse<List<CourseStatusDto>>> GetAllCourseStatuses()
    {
        var cache = await cacheRepository.GetAsync(Constants.CourseStatusPrefix);
        
        if (cache != null)
        {
            var deserializedCourseStatuses = JsonSerializer.Deserialize<List<CourseStatusDto>?>(cache);
            return MethodResponse<List<CourseStatusDto>>.Success(deserializedCourseStatuses!);
        }
        
        var courseStatuses = await courseStatusRepository.GetAllAsync(1, 100);
        
        if (courseStatuses == null || courseStatuses.Count <= 0)
        {
            logger.LogError($"Failed to get course statuses");
            return MethodResponse<List<CourseStatusDto>>.Failure(
                new Error(ErrorConstants.CourseStatusesNotFound, "Course statuses were not found")
            );
        }
        
        var courseStatusDtos = CourseStatusDto.ToDtoList(courseStatuses);
        var serializedCourseStatusDtos = JsonSerializer.Serialize(courseStatusDtos);
        await cacheRepository.SetAsync(Constants.CourseStatusPrefix, 
            serializedCourseStatusDtos, Constants.ExtraLongCachePeriod);
        
        logger.LogInformation($"Successfully retrieved course statuses");
        return MethodResponse<List<CourseStatusDto>>.Success(courseStatusDtos);
    }
    
    public async Task<MethodResponse<List<CourseDto>>> GetCoursesByUserAsync(Guid userId, int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.CoursePrefix + 
                                                        Constants.UserPrefix + userId + pageNr + pageSize);
        if (cache != null)
        {
            var deserializedCourses = JsonSerializer.Deserialize<List<CourseDto>?>(cache);
            return MethodResponse<List<CourseDto>>.Success(deserializedCourses!);
        }
        
        var coursesByUser = await courseRepository.GetAllByUserAsync(userId, pageNr, pageSize);
        if (coursesByUser == null)
        {
            logger.LogError($"Failed to get courses by user with ID {userId}");
            return MethodResponse<List<CourseDto>>.Failure(
                new Error(ErrorConstants.CoursesNotFound, "Courses were not found")
            );
        }
        var courseDtos = CourseDto.ToDtoList(coursesByUser);
        var serializedCourseDtos = JsonSerializer.Serialize(courseDtos);
        await cacheRepository.SetAsync(Constants.CoursePrefix + Constants.UserPrefix + userId + pageNr + pageSize, 
            serializedCourseDtos, Constants.ShortCachePeriod);
        
        logger.LogInformation($"Successfully retrieved courses by user with ID {userId}");
        return MethodResponse<List<CourseDto>>.Success(courseDtos);
    }
    
    public async Task<List<AttendanceStudentCountDto>?> GetAttendancesUserCountsByCourseAsync(Guid courseId)
    {
        var cache = await cacheRepository.GetAsync(Constants.CourseStudentCountsPrefix + courseId);

        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<AttendanceStudentCountDto>>(cache);
        }
        
        var studentCounts = await courseRepository.GetUserCountsAsync(courseId);
        if (studentCounts == null)
        {
            logger.LogError($"Failed to get attendances user counts by course with ID {courseId}");
            return null;
        }
        
        var serializedStudentCounts = JsonSerializer.Serialize(studentCounts);
        await cacheRepository.SetAsync(Constants.CourseStudentCountsPrefix + courseId, 
            serializedStudentCounts, Constants.ShortCachePeriod);

        logger.LogInformation("Successfully retrieved attendance user counts by course");
        var result = AttendanceStudentCountDto.AttendanceStudentCountDtos(studentCounts);
        return result;
    }
    
    private async Task<bool> DoesCourseExistByCodeAsync(string courseCode)
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
}

    // TODO: Implement soft deletion that cascade-soft-deletes CourseTeachers, CourseAttendances

    // TODO: Implement a method which can search and authenticate Courses that are soft deleted (IgnoreQueryFilters())
    
    // TODO: Implement restoration method that cascade-restores CourseTeachers, CourseAttendances
    
    // TODO: Implement an approach that can add multiple Teachers to one course
    
    // TODO: Implement an approach that can remove teachers
   
    