using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Sentry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.EFCore;

public class CourseTeacherRepository(AppDbContext context, ILogger<CourseTeacherRepository> logger, SentryService sentry) 
                                                                                                : ICourseTeacherRepository
{
    public async Task<List<CourseTeacherEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.CourseTeachers
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(ct => ct.Course)
                    .Include(ct => ct.Teacher)
                    .OrderBy(ct => ct.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.CourseTeachers
                    .AsNoTracking()
                    .Include(ct => ct.Course)
                    .Include(ct => ct.Teacher)
                    .OrderBy(ct => ct.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course teachers. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving course teachers. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.CourseTeachers
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.CourseTeachers
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting course teachers");
            sentry.CaptureWithContext(ex, "Error counting course teachers");
            return 0;
        }
    }

    public async Task<CourseTeacherEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.CourseTeachers
                    .Include(ct => ct.Teacher)
                    .Include(ct => ct.Course)
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct => ct.Id == id) : 
                await context.CourseTeachers
                    .Include(ct => ct.Teacher)
                    .Include(ct => ct.Course)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct => ct.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course teacher. CourseTeacher ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving course teacher. CourseTeacher ID: {0}", id);
            return null;
        }
    }

    public async Task<List<CourseTeacherEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.CourseTeachers
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(ct => ct.Course)
                    .Include(ct => ct.Teacher)
                    .Where(ct =>
                        ct.Course!.Name.ToLower().Contains(normalizedKeyword) ||
                        ct.Course.Code.ToLower().Contains(normalizedKeyword) ||
                        ct.Teacher!.FullName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ct => ct.Course!.Name)
                    .ThenBy(ct => ct.Teacher!.FullName)
                    .ToListAsync() : 
                await context.CourseTeachers
                    .AsNoTracking()
                    .Include(ct => ct.Course)
                    .Include(ct => ct.Teacher)
                    .Where(ct =>
                        ct.Course!.Name.ToLower().Contains(normalizedKeyword) ||
                        ct.Course.Code.ToLower().Contains(normalizedKeyword) ||
                        ct.Teacher!.FullName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ct => ct.Course!.Name)
                    .ThenBy(ct => ct.Teacher!.FullName)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching course teachers. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching course teachers. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<List<Guid>?> GetAllIdsByTeacherAsync(Guid userId)
    {
        try
        {
            return await context.CourseTeachers
                .IgnoreQueryFilters()
                .Where(ct => ct.TeacherId == userId)
                .Select(ct => ct.Id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all course teacher IDs by user. User ID: {UserId}", userId);
            sentry.CaptureWithContext(ex, "Error getting all course teacher IDs by user. User ID: {0}", userId);
            return null;    
        }    
    }

    public async Task<List<Guid>?> GetAllIdsByCourseAsync(Guid courseId)
    {
        try
        {
            return await context.CourseTeachers
                .IgnoreQueryFilters()
                .Where(ct => ct.CourseId == courseId)
                .Select(ct => ct.Id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all course teacher IDs by course. Course ID: {CserId}", courseId);
            sentry.CaptureWithContext(ex, "Error getting all course teacher IDs by course. Course ID: {0}", courseId);
            return null;    
        }        
    }

    public async Task<CourseTeacherEntity?> CreateAsync(CourseTeacherEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.CourseTeachers.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating course teacher. Course: {Course}", entity.CourseId);
            sentry.CaptureWithContext(ex, "Database error creating course teacher. Course: {0}", entity.CourseId);
            return null;
        }
    }

    public async Task<CourseTeacherEntity?> UpdateAsync(CourseTeacherEntity entity)
    {
        try
        {
            var exists = await context.CourseTeachers.IgnoreQueryFilters().AsNoTracking().AnyAsync(ct => ct.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.CourseTeachers.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating course teacher. CourseTeacher ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating course teacher. CourseTeacher ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating course teacher. CourseTeacher ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating course teacher. CourseTeacher ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState)
    {
        try
        {
            var affectedRows = await context.CourseTeachers
                .IgnoreQueryFilters()
                .Where(ac => ac.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Deleted, newDeletionState)
                    .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for course teacher. ID: {Id}, New State: {NewState}", 
                id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for course teacher. ID: {0}, New State: {1}", 
                id, newDeletionState);
            return false;
        }
    }

    public async Task<CourseTeacherEntity?> RemoveAsync(CourseTeacherEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.CourseTeachers.Attach(entity);
            }
        
            context.CourseTeachers.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing course teacher. CourseTeacher ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing course teacher. CourseTeacher ID: {0}", entity.Id);
            return null;
        }
    }
    
    public async Task<bool> ToggleDeletionForAllByTeacherAsync(Guid id, bool newDeletionState)
    {
        try
        {
            var affectedRows = await context.CourseTeachers
                .IgnoreQueryFilters()
                .Where(ac => ac.TeacherId == id)
                .ExecuteDeleteAsync();

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for course teacher. ID: {Id}, New State: {NewState}", 
                id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for course teacher. ID: {0}, New State: {1}", 
                id, newDeletionState);
            return false;
        }
    }
    
    public async Task<bool> ToggleDeletionForAllByCourseAsync(Guid id, bool newDeletionState)
    {
        try
        { 
            var affectedRows = await context.CourseTeachers
                .IgnoreQueryFilters()
                .Where(ac => ac.CourseId == id)
                .ExecuteDeleteAsync();;

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for course teacher. ID: {Id}, New State: {NewState}", 
                id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for course teacher. ID: {0}, New State: {1}", 
                id, newDeletionState);
            return false;
        }
    }
}