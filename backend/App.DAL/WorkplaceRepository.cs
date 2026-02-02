using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class WorkplaceRepository(AppDbContext context, ILogger<WorkplaceRepository> logger, SentryService sentry) : IWorkplaceRepository
{
    public async Task<List<WorkplaceEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .OrderBy(w => w.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Workplaces
                    .AsNoTracking()
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

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.Workplaces
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting workplaces");
            sentry.CaptureWithContext(ex, "Error counting workplaces");
            return 0;
        }
    }

    public async Task<WorkplaceEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == id) : 
                await context.Workplaces
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving workplace. Workplace ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving workplace. Workplace ID: {0}", id);
            return null;
        }    
    }

    public async Task<List<WorkplaceEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ?
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(w => w.Classroom)
                    .Where(w =>
                        w.Classroom!.Classroom.ToLower().Contains(normalizedKeyword) &&
                        (!resourceFilterId.HasValue || w.ClassroomId == resourceFilterId.Value))
                    .OrderBy(w => w.ComputerCode)
                    .ToListAsync() :
                await context.Workplaces
                    .AsNoTracking()
                    .Include(w => w.Classroom)
                    .Where(w =>
                        w.Classroom!.Classroom.ToLower().Contains(normalizedKeyword) &&
                        (!resourceFilterId.HasValue || w.ClassroomId == resourceFilterId.Value))
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
            var exists = await context.Workplaces.IgnoreQueryFilters().AsNoTracking().AnyAsync(w => w.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.Workplaces.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating workplace. Workplace ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating workplace. Workplace ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating workplace. Workplace ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating workplace. Workplace ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<WorkplaceEntity?> RemoveAsync(WorkplaceEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.Workplaces.Attach(entity);
            }
        
            context.Workplaces.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing workplace. Workplace ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing workplace. Workplace ID: {0}", entity.Id);
            return null;
        }
    }
    
    public async Task<Guid?> CheckAvailabilityByIdentifierAsync(string identifier, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Workplaces
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u => u.Identifier == identifier)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync() :
                await context.Workplaces
                    .AsNoTracking()
                    .Where(u => u.Identifier == identifier)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking workplace availability. Identifier: {Identifier}", identifier);
            sentry.CaptureWithContext(ex, "Error checking workplace availability. Identifier: {0}", identifier);
            return null;
        }
    }
}