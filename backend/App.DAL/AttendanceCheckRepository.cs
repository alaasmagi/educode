using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class AttendanceCheckRepository(AppDbContext context, ILogger<SchoolRepository> logger, SentryService sentry) : IAttendanceCheckRepository
{
    public async Task<List<AttendanceCheckEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceChecks
                    .IgnoreQueryFilters()
                    .OrderBy(ac => ac.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.AttendanceChecks
                    .OrderBy(ac => ac.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance checks. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving attendance checks. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }    
    }

    // TODO: INDEXING!
    public async Task<AttendanceCheckEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceChecks
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(ac => ac.Id == id) : 
                await context.AttendanceChecks
                    .FirstOrDefaultAsync(ac => ac.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance check. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving attendance check. ID: {0}", id);
            return null;
        }    
    }

    public async Task<List<AttendanceCheckEntity>?> SearchAsync(string keyword, bool includeDeleted = false)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.AttendanceChecks
                    .IgnoreQueryFilters()
                    .Where(ac =>
                        ac.AttendanceIdentifier.ToLower().Contains(normalizedKeyword) ||
                        ac.FullName.ToLower().Contains(normalizedKeyword) ||
                        ac.StudentCode.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ac => ac.AttendanceIdentifier)
                    .ToListAsync() : 
                await context.AttendanceChecks
                    .Where(ac =>
                        ac.AttendanceIdentifier.ToLower().Contains(normalizedKeyword) ||
                        ac.FullName.ToLower().Contains(normalizedKeyword) ||
                        ac.StudentCode.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ac => ac.AttendanceIdentifier)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching attendance checks. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching attendance checks. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<AttendanceCheckEntity?> CreateAsync(AttendanceCheckEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.AttendanceChecks.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating attendance check. StudentCode: {StudentCode}", entity.StudentCode);
            sentry.CaptureWithContext(ex, "Database error creating attendance check. StudentCode: {0}", entity.StudentCode);
            return null;
        }    
    }

    public async Task<AttendanceCheckEntity?> UpdateAsync(AttendanceCheckEntity entity)
    {
        try
        {
            var existingEntity = await context.AttendanceChecks.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.AttendanceIdentifier = entity.AttendanceIdentifier;
            existingEntity.FullName = entity.FullName;
            existingEntity.StudentCode = entity.StudentCode;
            existingEntity.CourseAttendance = entity.CourseAttendance;
            existingEntity.Workplace = entity.Workplace;
            existingEntity.WorkplaceIdentifier = entity.WorkplaceIdentifier;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating attendance check. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating attendance check. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating attendance check. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating attendance check. ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<AttendanceCheckEntity?> RemoveAsync(AttendanceCheckEntity entity)
    {
        try
        {
            context.AttendanceChecks.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing attendance check. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing attendance check. ID: {0}", entity.Id);
            return null;
        }
    }
}