using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class AttendanceRepository(AppDbContext context, ILogger<AttendanceTypeRepository> logger, SentryService sentry) 
                                                                                                : IAttendanceRepository
{ 
    public async Task<List<AttendanceEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .OrderBy(a => a.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Attendances
                    .OrderBy(a => a.Id)
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
    
    public async Task<List<AttendanceEntity>?> GetAllByCourseAsync(Guid courseId, int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .Where(a => a.CourseId == courseId)
                    .OrderBy(a => a.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Attendances
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
                    .Include(a => a.AttendanceType)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .FirstOrDefaultAsync(a => a.Id == id) : 
                await context.Attendances
                    .Include(a => a.AttendanceType)
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
                    .Include(a => a.AttendanceType)
                    .Include(a => a.Course)
                    .Include(a => a.Classroom)
                    .FirstOrDefaultAsync(a => a.Identifier == identifier) : 
                await context.Attendances
                    .Include(a => a.AttendanceType)
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
                .Where(ca => ca.StartTime <= DateTime.UtcNow && ca.EndTime >= DateTime.UtcNow &&
                             ca.Course!.CourseTeacherEntities!.Any(ct => ct.TeacherId == userId))
                .Include(a => a.AttendanceType)
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
                .Where(ca => ca.Course!.CourseTeacherEntities!
                    .Any(ct => ct.TeacherId == userId) && ca.StartTime <= DateTime.UtcNow) 
                .Include(a => a.AttendanceType)
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
    
    // TODO: INDEXING!
    public async Task<List<AttendanceEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            var query = includeDeleted 
                ? context.Attendances.IgnoreQueryFilters() 
                : context.Attendances;

            return await query
                .Include(a => a.Course)
                .ThenInclude(c => c!.CourseTeacherEntities!)
                .ThenInclude(ct => ct.Teacher)
                .Include(a => a.Classroom)
                .ThenInclude(cl => cl!.School)
                .Include(a => a.AttendanceType)
                .Where(a =>
                    (a.Course != null && a.Course.CourseName.ToLower().Contains(normalizedKeyword)) ||
                    (a.Course != null && a.Course.CourseCode.ToLower().Contains(normalizedKeyword)) ||
                    (a.Course != null && a.Course.CourseTeacherEntities != null && 
                     a.Course.CourseTeacherEntities.Any(ct => 
                         ct.Teacher != null && ct.Teacher.FullName.ToLower().Contains(normalizedKeyword))) ||
                    (a.Classroom != null && a.Classroom.Classroom.ToLower().Contains(normalizedKeyword)) ||
                    (a.Classroom != null && a.Classroom.School != null && 
                     a.Classroom.School.Name.ToLower().Contains(normalizedKeyword)) ||
                    (a.AttendanceType != null && a.AttendanceType.AttendanceType.ToLower().Contains(normalizedKeyword)) ||
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
            var existingEntity = await context.Attendances.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.AutomatedRegistration = entity.AutomatedRegistration;
            existingEntity.CourseId = entity.CourseId;
            existingEntity.Identifier = entity.Identifier;
            existingEntity.StartTime = entity.StartTime;
            existingEntity.EndTime = entity.EndTime;
            existingEntity.ClassroomId = entity.ClassroomId;
            existingEntity.AttendanceTypeId = entity.AttendanceTypeId;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
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

    public async Task<AttendanceEntity?> RemoveAsync(AttendanceEntity entity)
    {
        try
        {
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
    
    
    // TODO: MOVE TO SERVICE LAYER
    public void SeedAttendanceTypes(List<AttendanceTypeEntity> attendanceTypes)
    {
        if (!context.AttendanceTypes.Any())
        {
            context.AttendanceTypes.AddRange(attendanceTypes);
            context.SaveChanges();
        }
    }
    
    
}