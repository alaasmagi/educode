using App.Common;
using App.DAL.Contracts;
using App.Domain;
using App.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class CourseRepository(AppDbContext context, ILogger<CourseRepository> logger, SentryService sentry) : ICourseRepository
{
   public async Task<List<CourseEntity>?> GetCoursesByUser(Guid userId, int pageNr, int pageSize)
    {
        var result = await context.Courses
            .Where(ca => ca.CourseTeacherEntities!
                .Any(ct => ct.TeacherId == userId))
            .OrderBy(c => c.Id)
            .Skip((pageNr - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return result.Count > 0 ? result : null; 
    }
    
    public async Task<bool> AddCourseEntity(CourseTeacherEntity teacher, CourseEntity newCourse)
    {
        newCourse.CreatedAt = DateTime.UtcNow;
        newCourse.UpdatedAt = DateTime.UtcNow;
        
        await context.Courses.AddAsync(newCourse);
        
        teacher.CourseId = newCourse.Id;
        teacher.Course = newCourse;
        teacher.CreatedAt = DateTime.UtcNow;
        teacher.UpdatedAt = DateTime.UtcNow;
        
        await context.CourseTeachers.AddAsync(teacher);
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdateCourseEntity(Guid courseId, CourseEntity updatedCourse)
    {
        var course = await context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null)
        {
            return false;
        }

        course.CourseName = updatedCourse.CourseName;
        course.CourseCode = updatedCourse.CourseCode;
        course.CourseStatusId = updatedCourse.CourseStatusId;
        course.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteCourseEntity(CourseEntity course)
    {
        context.Courses.Remove(course);
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<List<AttendanceStudentCountDto>?> GetAllUserCountsByCourseId(Guid courseId)
    {
        var courseExists = await context.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            return null;

        var attendances = await context.Attendances
            .Where(ca => ca.CourseId == courseId)
            .ToListAsync();

        var result = new List<AttendanceStudentCountDto>();

        foreach (var attendance in attendances)
        {
            var count = await context.AttendanceChecks
                .CountAsync(ac => ac.AttendanceIdentifier == attendance.Identifier);

            result.Add(new AttendanceStudentCountDto
            {
                AttendanceDate = attendance.StartTime,
                StudentCount = count
            });
        }

        return result;
    }

    public async Task<bool> CourseAvailabilityCheckByCourseCode(string courseCode)
    {
        return await context.Courses.AnyAsync(c => c.CourseCode == courseCode);
    }
    
    public async Task<bool> CourseAvailabilityCheckById(Guid id)
    {
        return await context.Courses.AnyAsync(c => c.Id == id);
    }
    
    public async Task<CourseEntity?> GetCourseById(Guid courseId)
    {
        return await context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
    }
    
    public async Task<CourseEntity?> GetCourseByName(string courseName)
    {
        return await context.Courses.FirstOrDefaultAsync(c => c.CourseName == courseName);
    }
    
    public async Task<CourseEntity?> GetCourseByCode(string courseCode)
    {
        return await context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
    }

    public async Task<int> CourseAccessibilityCheck(Guid courseId, Guid userId)
    {
        return await context.CourseTeachers
            .CountAsync(ct => ct.TeacherId == userId && ct.CourseId == courseId);
    }
    public async Task<List<CourseStatusEntity>?> GetAllCourseStatuses()
    {
        return await context.CourseStatuses.ToListAsync();
    }
    
    public async Task<bool> CourseOnlyTeacherCheck(Guid userId, Guid courseId)
    {
        var courseTeachers = await context.CourseTeachers.Where(c => c.CourseId == courseId).ToListAsync();

        if (courseTeachers.Count == 1 && courseTeachers[0].TeacherId == userId)
        {
            return true;
        }

        return false;
    }
    
    public async Task<bool> RemoveOldCourses(DateTime datePeriod)
    {
        return await context.Courses
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldCourseStatuses(DateTime datePeriod)
    {
        return await context.CourseStatuses
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldCourseTeachers(DateTime datePeriod)
    {
        return await context.CourseTeachers
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public void SeedCourseStatuses(List<CourseStatusEntity> courseStatuses)
    {
        if (!context.CourseStatuses.Any())
        {
            context.CourseStatuses.AddRange(courseStatuses);
            context.SaveChanges();
        }
    }
    
    public async Task DeleteCoursesByUserAsync(Guid userId)
    {
        await context.Courses
            .Where(ca => ca.CourseTeacherEntities!.Any(ct => ct.TeacherId == userId))
            .ExecuteDeleteAsync();
    }

    public async Task<List<CourseEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .Include(c => c.CourseStatus)
                    .Include(c => c.CourseTeacherEntities!)
                    .ThenInclude(ct => ct.Teacher)
                    .OrderBy(c => c.CourseName)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Courses.IgnoreQueryFilters()
                    .Include(c => c.CourseStatus)
                    .Include(c => c.CourseTeacherEntities!)
                    .ThenInclude(ct => ct.Teacher)
                    .OrderBy(c => c.CourseName)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting courses. PageNr: {PageNr}, PageSize: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error getting courses. PageNr: {0}, PageSize: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<CourseEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .Include(c => c.CourseStatus)
                    .Include(c => c.CourseTeacherEntities!)
                    .ThenInclude(ct => ct.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == id) : 
                await context.Courses
                    .Include(c => c.CourseStatus)
                    .Include(c => c.CourseTeacherEntities!)
                    .ThenInclude(ct => ct.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course by ID. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error getting course by ID. ID: {0}", id);
            return null;
        }
    }
    
    public async Task<List<CourseEntity>?> SearchAsync(string keyword, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ?
                await context.Courses
                    .IgnoreQueryFilters()
                    .Include(c => c.CourseStatus)
                    .Include(c => c.CourseTeacherEntities!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(c => 
                        c.CourseName.ToLower().Contains(normalizedKeyword) || 
                        c.CourseCode.ToLower().Contains(normalizedKeyword) ||
                        (c.CourseStatus != null && c.CourseStatus.CourseStatus.ToLower().Contains(normalizedKeyword)) || 
                        (c.CourseTeacherEntities != null && 
                         c.CourseTeacherEntities.Any(ct =>
                             ct.Teacher != null && ct.Teacher.FullName.ToLower().Contains(normalizedKeyword))))
                    .OrderBy(c => c.CourseName)
                    .ToListAsync() : 
                await context.Courses
                    .Include(c => c.CourseStatus)
                    .Include(c => c.CourseTeacherEntities!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(c => 
                        c.CourseName.ToLower().Contains(normalizedKeyword) || 
                        c.CourseCode.ToLower().Contains(normalizedKeyword) ||
                        (c.CourseStatus != null && c.CourseStatus.CourseStatus.ToLower().Contains(normalizedKeyword)) || 
                        (c.CourseTeacherEntities != null && 
                         c.CourseTeacherEntities.Any(ct =>
                             ct.Teacher != null && ct.Teacher.FullName.ToLower().Contains(normalizedKeyword))))
                    .OrderBy(c => c.CourseName)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching courses. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching courses. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<CourseEntity?> CreateAsync(CourseEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await context.Courses.AddAsync(entity);
            await context.SaveChangesAsync();

            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course. Course: {Course}", entity.CourseName);
            sentry.CaptureWithContext(ex, "Error creating course. Course: {0}", entity.CourseName);
            return null;
        }
    }

    public async Task<CourseEntity?> UpdateAsync(CourseEntity entity)
    {
        try
        {
            var existingEntity = await context.Courses.FirstOrDefaultAsync(c => c.Id == entity.Id);
            if (existingEntity == null)
            {
                return null;
            }

            existingEntity.CourseName = entity.CourseName;
            existingEntity.CourseCode = entity.CourseCode;
            existingEntity.CourseStatusId = entity.CourseStatusId;
            existingEntity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return existingEntity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Error updating course. ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<CourseEntity?> RemoveAsync(CourseEntity entity)
    {
        try
        {
            context.Courses.Remove(entity);
            await context.SaveChangesAsync();

            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing course. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Error removing course. ID: {0}", entity.Id);
            return null;
        }
    }
}