using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class CourseStatusRepository(AppDbContext context, ILogger<CourseStatusRepository> logger, SentryService sentry)
                                                                                                : ICourseStatusRepository
{
    public async Task<List<CourseStatusEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.CourseStatuses
                    .IgnoreQueryFilters()
                    .OrderBy(cs => cs.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.CourseStatuses
                    .OrderBy(cs => cs.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course statuses. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving course statuses. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<CourseStatusEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.CourseStatuses
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(cs => cs.Id == id) : 
                await context.CourseStatuses
                    .FirstOrDefaultAsync(cs => cs.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course status. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving course status. ID: {0}", id);
            return null;
        }
    }

    public async Task<List<CourseStatusEntity>?> SearchAsync(string keyword, bool includeDeleted = false)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.CourseStatuses
                    .IgnoreQueryFilters()
                    .Where(cs =>
                        cs.CourseStatus.ToLower().Contains(normalizedKeyword))
                    .OrderBy(cs => cs.CourseStatus)
                    .ToListAsync() : 
                await context.CourseStatuses
                    .Where(cs =>
                        cs.CourseStatus.ToLower().Contains(normalizedKeyword))
                    .OrderBy(cs => cs.CourseStatus)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching course status. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching course status. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<CourseStatusEntity?> CreateAsync(CourseStatusEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.CourseStatuses.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating course status. CourseStatus: {CourseStatus}", entity.CourseStatus);
            sentry.CaptureWithContext(ex, "Database error creating course status. CourseStatus: {0}", entity.CourseStatus);
            return null;
        }
    }

    public async Task<CourseStatusEntity?> UpdateAsync(CourseStatusEntity entity)
    {
        try
        {
            var existingEntity = await context.CourseStatuses.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.CourseStatus = entity.CourseStatus;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating course status. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating course status. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating course status. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating course status. ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<CourseStatusEntity?> RemoveAsync(CourseStatusEntity entity)
    {
        try
        {
            context.CourseStatuses.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing course status. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing course status. ID: {0}", entity.Id);
            return null;
        }
    }
}