using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Sentry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.EFCore;

public class ClassroomRepository(
    AppDbContext context, 
    ILogger<ClassroomRepository> logger, 
    SentryService sentry) : IClassroomRepository
{
    public async Task<List<ClassroomEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Classrooms
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .OrderBy(c => c.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Classrooms
                    .AsNoTracking()
                    .OrderBy(c => c.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving classrooms. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving classrooms. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Classrooms
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.Classrooms
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting classrooms");
            sentry.CaptureWithContext(ex, "Error counting classrooms");
            return 0;
        }
    }

    public async Task<ClassroomEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Classrooms
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id) : 
                await context.Classrooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving classroom. Classroom ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving classroom. Classroom ID: {0}", id);
            return null;
        }
    }

    public async Task<List<ClassroomEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.Classrooms
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(c => c.School)
                    .Where(c =>
                        c.Classroom.ToLower().Contains(normalizedKeyword) ||
                        c.School!.Name.ToLower().Contains(normalizedKeyword))
                    .OrderBy(c => c.School!.Name)
                    .ThenBy(c => c.Classroom)
                    .ToListAsync() : 
                await context.Classrooms
                    .AsNoTracking()
                    .Include(c => c.School)
                    .Where(c =>
                        c.Classroom.ToLower().Contains(normalizedKeyword) ||
                        c.School!.Name.ToLower().Contains(normalizedKeyword))
                    .OrderBy(c => c.School!.Name)
                    .ThenBy(c => c.Classroom)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching classrooms. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching classrooms. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<ClassroomEntity?> CreateAsync(ClassroomEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.Classrooms.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating classroom. Classroom: {Classroom}", entity.Classroom);
            sentry.CaptureWithContext(ex, "Database error creating classroom. Classroom: {0}", entity.Classroom);
            return null;
        }
    }

    public async Task<ClassroomEntity?> UpdateAsync(ClassroomEntity entity)
    {
        try
        {
            var exists = await context.Classrooms
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(c => c.Id == entity.Id);
            
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.Classrooms.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating classroom. Classroom ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating classroom. Classroom ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating classroom. Classroom ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating classroom. Classroom ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState)
    {
        try
        {
            var affectedRows = await context.Classrooms
                .IgnoreQueryFilters()
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.Deleted, newDeletionState)
                    .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for classroom. ID: {Id}, New State: {NewState}", 
                                                                                                    id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for classroom. ID: {0}, New State: {1}", 
                                                                                                    id, newDeletionState);
            return false;
        }
    }

    public async Task<ClassroomEntity?> RemoveAsync(ClassroomEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.Classrooms.Attach(entity);
            }
        
            context.Classrooms.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing classroom. Classroom ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing classroom. Classroom ID: {0}", entity.Id);
            return null;
        }
    }
}