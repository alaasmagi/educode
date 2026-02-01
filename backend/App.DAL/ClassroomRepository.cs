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

public class ClassroomRepository(AppDbContext context, ILogger<ClassroomRepository> logger, SentryService sentry) : IClassroomRepository
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
            var exists = await context.Classrooms.IgnoreQueryFilters().AsNoTracking().AnyAsync(c => c.Id == entity.Id);
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