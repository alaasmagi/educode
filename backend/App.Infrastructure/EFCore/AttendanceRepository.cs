using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Sentry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.EFCore;

public class AttendanceRepository(
    AppDbContext context, 
    ILogger<AttendanceTypeRepository> logger, 
    SentryService sentry) : IAttendanceRepository
{ 
    public async Task<List<AttendanceEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .OrderBy(a => a.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Attendances
                    .AsNoTracking()
                    .OrderBy(a => a.Id)
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendances. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving attendances. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }
    
    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.Attendances
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting attendances");
            sentry.CaptureWithContext(ex, "Error counting attendances");
            return 0;
        }
    }
    
    public async Task<List<AttendanceEntity>?> GetAllByCourseAsync(Guid courseId, int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .Where(a => a.CourseId == courseId)
                    .OrderBy(a => a.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Attendances
                    .AsNoTracking()
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .Where(a => a.CourseId == courseId)
                    .OrderBy(a => a.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendances by Course. Course ID: {CourseId}, Page: {PageNr}, " +
                                                                            "Size: {PageSize}", courseId, pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving attendances by Course. Course ID: {0}, Page: {1}, " +
                                                                                "Size: {2}", courseId, pageNr, pageSize);
            return null;
        }
    }
    
    public async Task<AttendanceEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .FirstOrDefaultAsync(a => a.Id == id) : 
                await context.Attendances
                    .AsNoTracking()
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance. Attendance ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving attendance. Attendance ID: {0}", id);
            return null;
        }        
    }
    
    public async Task<AttendanceEntity?> GetByIdentifierAsync(string identifier, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .FirstOrDefaultAsync(a => a.Identifier == identifier) : 
                await context.Attendances
                    .AsNoTracking()
                    .Include(a => a.Type)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .FirstOrDefaultAsync(a => a.Identifier == identifier);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance. Attendance Identifier: {Identifier}", identifier);
            sentry.CaptureWithContext(ex, "Error retrieving attendance. Attendance Identifier: {Identifier}", identifier);
            return null;
        }
    }
    
    public async Task<AttendanceEntity?> GetOngoingByUserAsync(Guid userId)
    {
        try
        {
            return await context.Attendances
                .AsNoTracking()
                .Where(ca => ca.StartTime <= DateTime.UtcNow && ca.EndTime >= DateTime.UtcNow &&
                             ca.Course!.Teachers!.Any(ct => ct.TeacherId == userId))
                .Include(a => a.Type)
                .Include(a => a.Course)
                .Include(a => a.Classroom)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving ongoing attendance by user. User ID: {UserId}", userId);
            sentry.CaptureWithContext(ex, "Error retrieving ongoing attendance by user. User ID: {0}", userId);
            return null;
        }
    }
    
    public async Task<AttendanceEntity?> GetMostRecentByUserAsync(Guid userId)
    {
        try
        {
            return await context.Attendances
                .AsNoTracking()
                .Where(ca => ca.Course!.Teachers!
                    .Any(ct => ct.TeacherId == userId) && ca.StartTime <= DateTime.UtcNow) 
                .Include(a => a.Type)
                .Include(a => a.Course)
                .Include(a => a.Classroom) 
                .OrderByDescending(ca => ca.EndTime)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving most recent attendance by user. User ID: {UserId}", userId);
            sentry.CaptureWithContext(ex, "Error retrieving most recent attendance by user. User ID: {0}", userId);
            return null;
        }
    }
    
    public async Task<List<AttendanceEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            var query = includeDeleted 
                ? context.Attendances.IgnoreQueryFilters().AsNoTracking() 
                : context.Attendances.AsNoTracking();

            return await query
                .Include(a => a.Course)
                .ThenInclude(c => c!.Teachers!)
                .ThenInclude(ct => ct.Teacher)
                .Include(a => a.Classroom)
                .ThenInclude(cl => cl!.School)
                .Include(a => a.Type)
                .Where(a =>
                    (a.Course != null && a.Course.Name.ToLower().Contains(normalizedKeyword)) ||
                    (a.Course != null && a.Course.Name.ToLower().Contains(normalizedKeyword)) ||
                    (a.Course != null && a.Course.Teachers != null && 
                     a.Course.Teachers.Any(ct => 
                         ct.Teacher != null && ct.Teacher.FullName.ToLower().Contains(normalizedKeyword))) ||
                    (a.Classroom != null && a.Classroom.Classroom.ToLower().Contains(normalizedKeyword)) ||
                    (a.Classroom != null && a.Classroom.School != null && 
                     a.Classroom.School.Name.ToLower().Contains(normalizedKeyword)) ||
                    (a.Type != null && a.Type.TypeName.ToLower().Contains(normalizedKeyword)) ||
                    a.Identifier.ToLower().Contains(normalizedKeyword))
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching attendances. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching attendances. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<AttendanceEntity?> CreateAsync(AttendanceEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.Attendances.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating attendance. Attendance Identifier: {Identifier}", entity.Identifier);
            sentry.CaptureWithContext(ex, "Database error creating attendance. Attendance Identifier: {0}", entity.Identifier);
            return null;
        }   
    }

    public async Task<AttendanceEntity?> UpdateAsync(AttendanceEntity entity)
    {
        try
        {
            var exists = await context.Attendances
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(a => a.Id == entity.Id);
            
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.Attendances.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating attendance. Attendance ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating attendance. Attendance ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating attendance. Attendance ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating attendance. Attendance ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState)
    {
        try
        {
            var affectedRows = await context.Attendances
                .IgnoreQueryFilters()
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Deleted, newDeletionState)
                    .SetProperty(a => a.UpdatedAt, DateTime.UtcNow));
            
            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for attendance. ID: {Id}, New State: {NewState}", 
                                                                                                    id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for attendance. ID: {0}, " +
                                                                                "New State: {1}", id, newDeletionState);
            return false;
        }
    }

    public async Task<AttendanceEntity?> RemoveAsync(AttendanceEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.Attendances.Attach(entity);
            }
        
            context.Attendances.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing attendance. Attendance ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing attendance. Attendance ID: {0}", entity.Id);
            return null;
        }    
    }
    
    public async Task<Guid?> CheckAvailabilityByIdentifierAsync(string identifier, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u => u.Identifier == identifier)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync() :
                await context.Attendances
                    .AsNoTracking()
                    .Where(u => u.Identifier == identifier)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking attendance availability. Identifier: {Identifier}", identifier);
            sentry.CaptureWithContext(ex, "Error checking attendance availability. Identifier: {0}", identifier);
            return null;
        }
    }
}