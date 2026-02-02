using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class AttendanceCheckRepository(AppDbContext context, ILogger<AttendanceCheckRepository> logger, SentryService sentry)
                                                                                            : IAttendanceCheckRepository
{
    public async Task<List<AttendanceCheckEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceChecks
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(ac => ac.Attendance)
                    .Include(ac => ac.Workplace)
                    .OrderBy(ac => ac.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.AttendanceChecks
                    .AsNoTracking()
                    .OrderBy(ac => ac.Id)
                    .Include(ac => ac.Attendance)
                    .Include(ac => ac.Workplace)
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
    
    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceChecks
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.AttendanceChecks
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting attendance checks");
            sentry.CaptureWithContext(ex, "Error counting attendance checks");
            return 0;
        }
    }
    
    public async Task<List<AttendanceCheckEntity>?> GetAllByAttendanceAsync(Guid attendanceId)
    {
        try
        {
            return await context.AttendanceChecks
                .AsNoTracking()
                .Where(c => c.Attendance!.Id == attendanceId)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance checks by attendance. Attendance ID: {AttendanceId}", attendanceId);
            sentry.CaptureWithContext(ex, "Error retrieving attendance checks by attendance. Attendance ID: {0}", attendanceId);
            return null;
        }
    }
    
    public async Task<List<AttendanceCheckEntity>?> GetAllByAttendanceIdentifierAsync(string attendanceIdentifier, 
                                                                                                int pageNr, int pageSize)
    {
        try
        {
            return await context.AttendanceChecks
                .AsNoTracking()
                .Where(c => c.AttendanceIdentifier == attendanceIdentifier)
                .OrderBy(c => c.Id)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance checks by attendance. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving attendance checks by attendance. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }
    
    public async Task<int?> GetUserCountAsync(Guid attendanceId)
    {
        try
        {
            return await context.AttendanceChecks
                .AsNoTracking()
                .Where(a => a.Id == attendanceId)
                .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user count by attendance. Attendance ID: {AttendanceId}", attendanceId);
            sentry.CaptureWithContext(ex, "Error retrieving user count by attendance. Attendance ID: {0}", attendanceId);
            return null;
        }
    }

    public async Task<AttendanceCheckEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.AttendanceChecks
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(ac => ac.Attendance)
                    .Include(ac => ac.Workplace)
                    .FirstOrDefaultAsync(ac => ac.Id == id) : 
                await context.AttendanceChecks
                    .AsNoTracking()
                    .Include(ac => ac.Attendance)
                    .Include(ac => ac.Workplace)
                    .FirstOrDefaultAsync(ac => ac.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance check. AttendanceCheck ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving attendance check. AttendanceCheck ID: {0}", id);
            return null;
        }    
    }

    public async Task<List<AttendanceCheckEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.AttendanceChecks
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(ac =>
                        ac.AttendanceIdentifier.ToLower().Contains(normalizedKeyword) ||
                        ac.FullName.ToLower().Contains(normalizedKeyword) ||
                        ac.StudentCode.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ac => ac.AttendanceIdentifier)
                    .ToListAsync() : 
                await context.AttendanceChecks
                    .AsNoTracking()
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
            var exists = await context.AttendanceChecks.IgnoreQueryFilters().AsNoTracking().AnyAsync(ac => ac.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.AttendanceChecks.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating attendance check. AttendanceCheck ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating attendance check. AttendanceCheck ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating attendance check. AttendanceCheck ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating attendance check. AttendanceCheck ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<AttendanceCheckEntity?> RemoveAsync(AttendanceCheckEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.AttendanceChecks.Attach(entity);
            }
        
            context.AttendanceChecks.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing attendance check. AttendanceCheck ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing attendance check. AttendanceCheck ID: {0}", entity.Id);
            return null;
        }
    }
    
}