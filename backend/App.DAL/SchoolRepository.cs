using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public abstract class SchoolRepository(AppDbContext context, ILogger<SchoolRepository> logger, SentryService sentry) : ISchoolRepository
{ 
    public async Task<List<SchoolEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Schools
                    .IgnoreQueryFilters()
                    .OrderBy(s => s.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Schools
                    .OrderBy(s => s.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving schools. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving schools. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<SchoolEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Schools
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Id == id) : 
                await context.Schools
                    .FirstOrDefaultAsync(s => s.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving school. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving school. ID: {0}", id);
            return null;
        }
    }

    // TODO: INDEXING!
    public async Task<List<SchoolEntity>?> SearchAsync(string keyword, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();
            
            return includeDeleted ? 
                await context.Schools
                    .IgnoreQueryFilters()
                    .Where(s =>
                        s.Name.ToLower().Contains(normalizedKeyword) ||
                        s.ShortName.ToLower().Contains(normalizedKeyword) ||
                        s.Domain.ToLower().Contains(normalizedKeyword))
                    .OrderBy(s => s.Name)
                    .ToListAsync() : 
                await context.Schools
                    .Where(s =>
                        s.Name.ToLower().Contains(normalizedKeyword) ||
                        s.ShortName.ToLower().Contains(normalizedKeyword) || 
                        s.Domain.ToLower().Contains(normalizedKeyword))
                    .OrderBy(s => s.Name)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching schools. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching schools. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<SchoolEntity?> CreateAsync(SchoolEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.Schools.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating school. SchoolName: {SchoolName}", entity.Name);
            sentry.CaptureWithContext(ex, "Database error creating school. SchoolName: {0}", entity.Name);
            return null;
        }
    }

    public async Task<SchoolEntity?> UpdateAsync(SchoolEntity entity)
    {
        try
        {
            var existingEntity = await context.Schools.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.Name = entity.Name;
            existingEntity.ShortName = entity.ShortName;
            existingEntity.Domain = entity.Domain;
            existingEntity.StudentCodePattern = entity.StudentCodePattern;
            existingEntity.Deleted = entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating school. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating school. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating school. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating school. ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<SchoolEntity?> RemoveAsync(SchoolEntity entity)
    {
        try
        {
            context.Schools.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing school. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing school. ID: {0}", entity.Id);
            return null;
        }
    }
}