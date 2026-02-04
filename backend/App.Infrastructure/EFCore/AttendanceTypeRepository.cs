using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Sentry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.EFCore;

public class AttendanceTypeRepository(
    AppDbContext context, 
    ILogger<AttendanceTypeRepository> logger, 
    SentryService sentry) : IAttendanceTypeRepository
{
    public async Task<List<AttendanceTypeEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .OrderBy(at => at.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.AttendanceTypes
                    .AsNoTracking()
                    .OrderBy(at => at.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance types. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving attendance types. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.AttendanceTypes
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting attendance types");
            sentry.CaptureWithContext(ex, "Error counting attendance types");
            return 0;
        }
    }

    public async Task<AttendanceTypeEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(at => at.Id == id) : 
                await context.AttendanceTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(at => at.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance type. AttendanceCheck ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving attendance type. AttendanceCheck ID: {0}", id);
            return null;
        }        
    }

    public async Task<List<AttendanceTypeEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.AttendanceTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(at =>
                        at.TypeName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(at => at.TypeName)
                    .ToListAsync() : 
                await context.AttendanceTypes
                    .AsNoTracking()
                    .Where(at =>
                        at.TypeName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching attendance types. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching attendance types. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<AttendanceTypeEntity?> CreateAsync(AttendanceTypeEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.AttendanceTypes.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating attendance type. AttendanceType: {TypeName}", entity.TypeName);
            sentry.CaptureWithContext(ex, "Database error creating attendance type. AttendanceType: {0}", entity.TypeName);
            return null;
        }        
    }

    public async Task<AttendanceTypeEntity?> UpdateAsync(AttendanceTypeEntity entity)
    {
        try
        {
            var exists = await context.AttendanceTypes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(at => at.Id == entity.Id);
            
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.AttendanceTypes.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating attendance type. AttendanceCheck ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating attendance type. AttendanceCheck ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating attendance type. AttendanceCheck ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating attendance type. AttendanceCheck ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState)
    {
        try
        {
            var affectedRows = await context.AttendanceTypes
                .IgnoreQueryFilters()
                .Where(at => at.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(at => at.Deleted, newDeletionState)
                    .SetProperty(at => at.UpdatedAt, DateTime.UtcNow));

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for attendance type. ID: {Id}, New State: {NewState}", 
                                                                                                    id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for attendance type. ID: {0}, New State: {1}", 
                                                                                                    id, newDeletionState);
            return false;
        }
    }

    public async Task<AttendanceTypeEntity?> RemoveAsync(AttendanceTypeEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.AttendanceTypes.Attach(entity);
            }
        
            context.AttendanceTypes.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing attendance type. AttendanceCheck ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing attendance type. AttendanceCheck ID: {0}", entity.Id);
            return null;
        }    
    }
}