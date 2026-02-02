using App.Common;
using App.DAL.Contracts;
using App.Domain;
using App.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class CourseRepository(AppDbContext context, ILogger<CourseRepository> logger, SentryService sentry) : ICourseRepository
{
    public async Task<List<CourseEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .OrderBy(c => c.Name)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Courses
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .OrderBy(c => c.Name)
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
    
    public async Task<List<CourseEntity>?> GetAllByUserAsync(Guid userId, int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(ca => ca.Teachers!
                        .Any(ct => ct.TeacherId == userId))                   
                    .OrderBy(c => c.Name)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Courses
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(ca => ca.Teachers!
                        .Any(ct => ct.TeacherId == userId))
                    .OrderBy(c => c.Name)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting courses by user. User ID: {UserId}, PageNr: {PageNr}, " +
                                                                        "PageSize: {PageSize}", userId, pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error getting courses by user. User ID: {0}, PageNr: {1}, PageSize: {2}", 
                                                                                                userId, pageNr, pageSize);
            return null;
        }
    }
    
    public async Task<List<CourseEntity>?> GetAllSingleUserByUserAsync(Guid userId, int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ?
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(ca => ca.Teachers!.Count == 1 && 
                                 ca.Teachers!.Any(ct => ct.TeacherId == userId))
                    .OrderBy(c => c.Name)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() :
                await context.Courses
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(ca => ca.Teachers!.Count == 1 && 
                                 ca.Teachers!.Any(ct => ct.TeacherId == userId))
                    .OrderBy(c => c.Name)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting single user courses by user. User ID: {UserId}, PageNr: {PageNr}, " + 
                                                                        "PageSize: {PageSize}", userId, pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error getting single user courses by user. User ID: {0}, PageNr: {1}, " +
                                                                                    "PageSize: {2}", userId, pageNr, pageSize);
            return null;
        }
    }
    
    public async Task<List<AttendanceStudentCountDto>?> GetUserCountsAsync(Guid id)
    {
        try
        {
            var attendances = await context.Attendances
                .AsNoTracking()
                .Where(ca => ca.CourseId == id)
                .ToListAsync();
            
            var result = new List<AttendanceStudentCountDto>();
            foreach (var attendance in attendances)
            {
                var count = await context.AttendanceChecks
                    .AsNoTracking()
                    .CountAsync(ac => ac.AttendanceIdentifier == attendance.Identifier);

                result.Add(new AttendanceStudentCountDto
                {
                    AttendanceId = attendance.Id,
                    AttendanceDate = attendance.StartTime,
                    StudentCount = count
                });
            }

            return result; 
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user counts for course. Course ID: {UserId}", id);
            sentry.CaptureWithContext(ex, "rror getting user counts for course. Course ID: {0}", id);
            return null;
        }
    }

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.Courses
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting courses");
            sentry.CaptureWithContext(ex, "Error counting courses");
            return 0;
        }
    }

    public async Task<CourseEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.School)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == id) : 
                await context.Courses
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.School)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting course. Course ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error getting course. Course ID: {0}", id);
            return null;
        }
    }
    
    public async Task<List<CourseEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ?
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(c => 
                        c.Name.ToLower().Contains(normalizedKeyword) || 
                        c.Code.ToLower().Contains(normalizedKeyword) ||
                        (c.Status != null && c.Status.StatusName.ToLower().Contains(normalizedKeyword)) || 
                        (c.Teachers != null && 
                         c.Teachers.Any(ct =>
                             ct.Teacher != null && ct.Teacher.FullName.ToLower().Contains(normalizedKeyword))))
                    .OrderBy(c => c.Name)
                    .ToListAsync() : 
                await context.Courses
                    .AsNoTracking()
                    .Include(c => c.Status)
                    .Include(c => c.Teachers!)
                    .ThenInclude(ct => ct.Teacher)
                    .Where(c => 
                        c.Name.ToLower().Contains(normalizedKeyword) || 
                        c.Code.ToLower().Contains(normalizedKeyword) ||
                        (c.Status != null && c.Status.StatusName.ToLower().Contains(normalizedKeyword)) || 
                        (c.Teachers != null && 
                         c.Teachers.Any(ct =>
                             ct.Teacher != null && ct.Teacher.FullName.ToLower().Contains(normalizedKeyword))))
                    .OrderBy(c => c.Name)
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
            logger.LogError(ex, "Error creating course. Course: {Course}", entity.Name);
            sentry.CaptureWithContext(ex, "Error creating course. Course: {0}", entity.Name);
            return null;
        }
    }

    public async Task<CourseEntity?> UpdateAsync(CourseEntity entity)
    {
        try
        {
            var exists = await context.Courses.IgnoreQueryFilters().AsNoTracking().AnyAsync(c => c.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.Courses.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course. Course ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Error updating course. Course ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<CourseEntity?> RemoveAsync(CourseEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.Courses.Attach(entity);
            }
        
            context.Courses.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing course. Course ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Error removing course. Course ID: {0}", entity.Id);
            return null;
        }
    }
    
    public async Task<Guid?> CheckAvailabilityByCodeAsync(string code, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u => u.Code == code)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync() :
                await context.Courses
                    .AsNoTracking()
                    .Where(u => u.Code == code)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking course availability. Course code: {CourseCode}", code);
            sentry.CaptureWithContext(ex, "Error checking course availability. Course code: {0}", code);
            return null;
        }
    }
    
    public async Task<Guid?> CheckAvailabilityByNameAsync(string name, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Courses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u => u.Name == name)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync() :
                await context.Courses
                    .AsNoTracking()
                    .Where(u => u.Name == name)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking course availability. Course name: {CourseName}", name);
            sentry.CaptureWithContext(ex, "Error checking course availability. Course name: {0}", name);
            return null;
        }
    }
    
    
    // TODO: Move to BLL
    public void SeedCourseStatuses(List<CourseStatusEntity> courseStatuses)
    {
        if (!context.CourseStatuses.Any())
        {
            context.CourseStatuses.AddRange(courseStatuses);
            context.SaveChanges();
        }
    }
    

}