using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class CourseStatusRepository(AppDbContext context, ILogger<CourseStatusRepository> logger, SentryService sentry)
                                                                                                : ICourseStatusRepository
{
    public async Task<List<CourseStatusEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
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

    public async Task<CourseStatusEntity?> GetByIdAsync(Guid id, bool includeDeleted)
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
            logger.LogError(ex, "Error retrieving course status. CourseStatus ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving course status. CourseStatus ID: {0}", id);
            return null;
        }
    }

    public async Task<List<CourseStatusEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.CourseStatuses
                    .IgnoreQueryFilters()
                    .Where(cs =>
                        cs.StatusName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(cs => cs.StatusName)
                    .ToListAsync() : 
                await context.CourseStatuses
                    .Where(cs =>
                        cs.StatusName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(cs => cs.StatusName)
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
            logger.LogError(ex, "Database error creating course status. CourseStatus: {CourseStatus}", entity.StatusName);
            sentry.CaptureWithContext(ex, "Database error creating course status. CourseStatus: {0}", entity.StatusName);
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
            
            existingEntity.StatusName = entity.StatusName;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating course status. CourseStatus ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating course status. CourseStatus ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating course status. CourseStatus ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating course status. CourseStatus ID: {0}", entity.Id);
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
            logger.LogError(ex, "Database error removing course status. CourseStatus ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing course status. CourseStatus ID: {0}", entity.Id);
            return null;
        }
    }
}