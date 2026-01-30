using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class WorkplaceRepository(AppDbContext context, ILogger<SchoolRepository> logger, SentryService sentry) : IWorkplaceRepository
{
    public async Task<List<WorkplaceEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .OrderBy(w => w.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Workplaces
                    .OrderBy(w => w.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving workplaces. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving workplaces. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<WorkplaceEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(w => w.Id == id) : 
                await context.Workplaces
                    .FirstOrDefaultAsync(w => w.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving workplace. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving workplace. ID: {0}", id);
            return null;
        }    
    }

    public async Task<List<WorkplaceEntity>?> SearchAsync(string keyword, bool includeDeleted = false)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .Include(w => w.Classroom)
                    .Where(w =>
                        w.Classroom!.Classroom.ToLower().Contains(normalizedKeyword))
                    .OrderBy(w => w.ComputerCode)
                    .ToListAsync() : 
                await context.Workplaces
                    .Where(w =>
                        w.Classroom!.Classroom.ToLower().Contains(normalizedKeyword))
                    .OrderBy(w => w.ComputerCode)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching workplaces. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching workplaces. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<WorkplaceEntity?> CreateAsync(WorkplaceEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.Workplaces.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating workplace. ComputerCode: {ComputerCode}", entity.ComputerCode);
            sentry.CaptureWithContext(ex, "Database error creating workplace. ComputerCode: {0}", entity.ComputerCode);
            return null;
        }    
    }

    public async Task<WorkplaceEntity?> UpdateAsync(WorkplaceEntity entity)
    {
        try
        {
            var existingEntity = await context.Workplaces.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.Identifier = entity.Identifier;
            existingEntity.ComputerCode = entity.ComputerCode;
            existingEntity.ClassroomId = entity.ClassroomId;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating workplace. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating workplace. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating workplace. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating workplace. ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<WorkplaceEntity?> RemoveAsync(WorkplaceEntity entity)
    {
        try
        {
            context.Workplaces.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing workplace. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing workplace. ID: {0}", entity.Id);
            return null;
        }
    }
}