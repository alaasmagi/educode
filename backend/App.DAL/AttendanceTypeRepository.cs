using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class AttendanceTypeRepository(AppDbContext context, ILogger<SchoolRepository> logger, SentryService sentry) : IAttendanceTypeRepository
{
    public async Task<List<AttendanceTypeEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceTypes
                    .IgnoreQueryFilters()
                    .OrderBy(at => at.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.AttendanceTypes
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

    public async Task<AttendanceTypeEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceTypes
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(at => at.Id == id) : 
                await context.AttendanceTypes
                    .FirstOrDefaultAsync(at => at.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance type. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving attendance type. ID: {0}", id);
            return null;
        }        
    }

    public async Task<List<AttendanceTypeEntity>?> SearchAsync(string keyword, bool includeDeleted = false)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.AttendanceTypes
                    .IgnoreQueryFilters()
                    .Where(at =>
                        at.AttendanceType.ToLower().Contains(normalizedKeyword))
                    .OrderBy(at => at.AttendanceType)
                    .ToListAsync() : 
                await context.AttendanceTypes
                    .Where(at =>
                        at.AttendanceType.ToLower().Contains(normalizedKeyword))
                    .OrderBy(at => at.AttendanceType)
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
            logger.LogError(ex, "Database error creating attendance type. AttendanceType: {AttendanceType}", entity.AttendanceType);
            sentry.CaptureWithContext(ex, "Database error creating attendance type. AttendanceType: {0}", entity.AttendanceType);
            return null;
        }        
    }

    public async Task<AttendanceTypeEntity?> UpdateAsync(AttendanceTypeEntity entity)
    {
        try
        {
            var existingEntity = await context.AttendanceTypes.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.AttendanceType = entity.AttendanceType;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating attendance type. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating attendance type. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating attendance type. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating attendance type. ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<AttendanceTypeEntity?> RemoveAsync(AttendanceTypeEntity entity)
    {
        try
        {
            context.AttendanceTypes.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing attendance type. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing attendance type. ID: {0}", entity.Id);
            return null;
        }    
    }
}