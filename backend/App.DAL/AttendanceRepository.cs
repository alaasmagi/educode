using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class AttendanceRepository(AppDbContext context, ILogger<AttendanceTypeRepository> logger, SentryService sentry) 
                                                                                                : IAttendanceRepository
{
    public async Task<bool> AddAttendanceCheck(AttendanceCheckEntity attendance, string creator, WorkplaceEntity? workplace)
    {
        attendance.CreatedBy = creator;
        attendance.UpdatedBy = creator;
        attendance.CreatedAt = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;

        if (workplace != null)
        {
            attendance.WorkplaceIdentifier = workplace.Identifier;
            attendance.Workplace = workplace;
        }
        
        await context.AttendanceChecks.AddAsync(attendance);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<AttendanceEntity?> GetCurrentAttendance(Guid userId)
    {
        var ongoingAttendance= await context.Attendances
            .Where(ca => ca.StartTime <= DateTime.UtcNow && ca.EndTime >= DateTime.UtcNow &&
                         ca.Course!.CourseTeacherEntities!.Any(ct => ct.TeacherId == userId)).
            Include(ca => ca.Course).Include(ca => ca.AttendanceType)
            .FirstOrDefaultAsync();
        
        return ongoingAttendance;
    }
    
    public async Task<bool> AddAttendance(AttendanceEntity attendance)
    {
        var doesAttendanceExist = context.Attendances.Any(ca => ca.CourseId == attendance.CourseId && 
                                                                ca.StartTime == attendance.StartTime && 
                                                                ca.EndTime == attendance.EndTime);

        if (doesAttendanceExist)
        {
            return false;
        }

        if (attendance.StartTime > attendance.EndTime)
        {
            return false;
        }

        attendance.CreatedAt = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;
        
        await context.Attendances.AddAsync(attendance);
       
        return await context.SaveChangesAsync() > 0 ;
    }
    
    public async Task<bool> UpdateAttendance(Guid attendanceId, AttendanceEntity updatedAttendance)
    {
        var attendance = await context.Attendances.FirstOrDefaultAsync(a => a.Id == attendanceId);
        if (attendance == null)
        {
            return false;
        }

        attendance.CourseId = updatedAttendance.CourseId;
        attendance.AttendanceTypeId = updatedAttendance.AttendanceTypeId;
        attendance.StartTime = updatedAttendance.StartTime;
        attendance.EndTime = updatedAttendance.EndTime;
        attendance.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteAttendanceEntity(AttendanceEntity attendanceEntity)
    {
        context.Attendances.Remove(attendanceEntity);
        return await context.SaveChangesAsync() > 0 ;
    }
    
    public async Task<bool> DeleteAttendanceCheckEntity(AttendanceCheckEntity attendanceCheckEntity)
    {
        context.AttendanceChecks.Remove(attendanceCheckEntity);
        return await context.SaveChangesAsync() > 0;
    }
    public async Task<int> GetStudentCountByAttendanceId(string attendanceIdentifier)
    {
        var attendanceCounts = await context.AttendanceChecks.Where(a => a.AttendanceIdentifier == attendanceIdentifier)
            .CountAsync();
        return attendanceCounts;
    }

    public async Task<AttendanceEntity?> GetAttendanceById(Guid attendanceId)
    {
        var attendance = await context.Attendances
            .Include(u => u.AttendanceType)
            .Include(u => u.Course)
            .FirstOrDefaultAsync(u => u.Id == attendanceId);

        if (attendance != null)
        {
            attendance.StartTime = DateTime.SpecifyKind(attendance.StartTime, DateTimeKind.Utc);
            attendance.EndTime = DateTime.SpecifyKind(attendance.EndTime, DateTimeKind.Utc);
        }

        return attendance;
    }
    
    public async Task<AttendanceEntity?> GetAttendanceByIdentifier(string attendanceIdentifier)
    {
        var attendance = await context.Attendances
            .Include(u => u.AttendanceType)
            .Include(u => u.Course)
            .FirstOrDefaultAsync(u => u.Identifier == attendanceIdentifier);

        if (attendance != null)
        {
            attendance.StartTime = DateTime.SpecifyKind(attendance.StartTime, DateTimeKind.Utc);
            attendance.EndTime = DateTime.SpecifyKind(attendance.EndTime, DateTimeKind.Utc);
        }

        return attendance;
    }

    public async Task<bool> WorkplaceAvailabilityCheckById(string workplaceIdentifier)
    {
        return await context.Workplaces.AnyAsync(w => w.Identifier == workplaceIdentifier);
    }
    
    public async Task<bool> WorkplaceAvailabilityCheckByIdentifier(string workplaceIdentifier)
    {
        return await context.Workplaces.AnyAsync(w => w.Identifier == workplaceIdentifier);
    }
    
    public async Task<WorkplaceEntity?> GetWorkplaceByIdentifier(string workplaceIdentifier)
    {
        return await context.Workplaces.FirstOrDefaultAsync(w => w.Identifier == workplaceIdentifier);
    }
    
    public async Task<bool> AttendanceAvailabilityCheckById(Guid attendanceId)
    {
        return await context.Attendances.AnyAsync(u => u.Id == attendanceId);
    }
    
    public async Task<bool> AttendanceCheckAvailabilityCheck(string studentCode, string attendanceIdentifier)
    {
        return await context.AttendanceChecks.AnyAsync(u => u.StudentCode == studentCode 
                                                              && u.AttendanceIdentifier == attendanceIdentifier);
    }
    
    public async Task<List<AttendanceEntity>> GetCourseAttendancesByCourseId(Guid courseId, int pageNr, int pageSize)
    {
        var attendances = await context.Attendances
            .Where(c => c.CourseId == courseId)
            .Include(c => c.Course)
            .OrderBy(c => c.Id)
            .Skip((pageNr - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        foreach (var attendance in attendances)
        {
            attendance.StartTime = DateTime.SpecifyKind(attendance.StartTime, DateTimeKind.Utc);
            attendance.EndTime = DateTime.SpecifyKind(attendance.EndTime, DateTimeKind.Utc);
        }

        return attendances;
    }

    public async Task<List<AttendanceCheckEntity>> GetAttendanceChecksByAttendanceIdentifier(string attendanceIdentifier, 
                                                                                                int pageNr, int pageSize)
    {
        return await context.AttendanceChecks
            .Where(c => c.AttendanceIdentifier == attendanceIdentifier)
            .OrderBy(c => c.Id)
            .Skip((pageNr - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<AttendanceCheckEntity?> GetAttendanceCheckById(Guid attendanceCheckId)
    {
        return await context.AttendanceChecks.FirstOrDefaultAsync(ca => ca.Id == attendanceCheckId);
    }
    
    
    public async Task<AttendanceEntity?> GetMostRecentAttendanceByUser(Guid userId)
    {
        return await context.Attendances
            .Where(ca => ca.Course!.CourseTeacherEntities!
                .Any(ct => ct.TeacherId == userId) && ca.StartTime <= DateTime.UtcNow) 
            .Include(ca => ca.Course)
            .Include(ca => ca.AttendanceType) 
            .OrderByDescending(ca => ca.EndTime)
            .FirstOrDefaultAsync();
    }
    
    public async Task<List<AttendanceTypeEntity>> GetAttendanceTypes()
    {
        return await context.AttendanceTypes.ToListAsync();
    }
    
    public async Task<AttendanceTypeEntity?> GetAttendanceTypeById(Guid attendanceTypeId)
    {
        return await context.AttendanceTypes
            .FirstOrDefaultAsync(ca => ca.Id == attendanceTypeId);
    }
    
    public async Task<bool> RemoveOldAttendances(DateTime datePeriod)
    {
        return await context.Attendances
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldAttendanceTypes(DateTime datePeriod)
    {
        return await context.AttendanceTypes
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldAttendanceChecks(DateTime datePeriod)
    {
        return await context.AttendanceChecks
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldWorkplaces(DateTime datePeriod)
    {
        return await context.Workplaces
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public void SeedAttendanceTypes(List<AttendanceTypeEntity> attendanceTypes)
    {
        if (!context.AttendanceTypes.Any())
        {
            context.AttendanceTypes.AddRange(attendanceTypes);
            context.SaveChanges();
        }
    }

    public async Task<List<AttendanceEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
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

    public async Task<AttendanceEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.Attendances
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a => a.Id == id) : 
                await context.Attendances
                    .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving attendance. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving attendance. ID: {0}", id);
            return null;
        }        
    }
    
    public async Task<List<AttendanceEntity>?> SearchAsync(string keyword, bool includeDeleted = false)
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
            logger.LogError(ex, "Database error creating attendance. Identifier: {Identifier}", entity.Identifier);
            sentry.CaptureWithContext(ex, "Database error creating attendance. Identifier: {0}", entity.Identifier);
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
            logger.LogError(ex, "Concurrency conflict updating attendance. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating attendance. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating attendance. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating attendance. ID: {0}", entity.Id);
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
            logger.LogError(ex, "Database error removing attendance. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing attendance. ID: {0}", entity.Id);
            return null;
        }    
    }
}