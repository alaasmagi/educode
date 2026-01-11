using App.Domain;
using App.DTO;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class CourseRepository(AppDbContext context)
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

        var attendances = await context.CourseAttendances
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
    
    public void SeedCourseStatuses()
    {
        if (!context.CourseStatuses.Any())
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
}