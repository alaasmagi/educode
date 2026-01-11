using System.Data;
using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class AttendanceRepository(AppDbContext context)
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

    public async Task<CourseAttendanceEntity?> GetCurrentAttendance(Guid userId)
    {
        var ongoingAttendance= await context.CourseAttendances
            .Where(ca => ca.StartTime <= DateTime.UtcNow && ca.EndTime >= DateTime.UtcNow &&
                         ca.Course!.CourseTeacherEntities!.Any(ct => ct.TeacherId == userId)).
            Include(ca => ca.Course).Include(ca => ca.AttendanceType)
            .FirstOrDefaultAsync();
        
        return ongoingAttendance;
    }
    
    public async Task<bool> AddAttendance(CourseAttendanceEntity attendance)
    {
        var doesAttendanceExist = context.CourseAttendances.Any(ca => ca.CourseId == attendance.CourseId && 
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
        
        await context.CourseAttendances.AddAsync(attendance);
       
        return await context.SaveChangesAsync() > 0 ;
    }
    
    public async Task<bool> UpdateAttendance(Guid attendanceId, CourseAttendanceEntity updatedAttendance)
    {
        var attendance = await context.CourseAttendances.FirstOrDefaultAsync(a => a.Id == attendanceId);
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
    
    public async Task<bool> DeleteAttendanceEntity(CourseAttendanceEntity attendanceEntity)
    {
        context.CourseAttendances.Remove(attendanceEntity);
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

    public async Task<CourseAttendanceEntity?> GetAttendanceById(Guid attendanceId)
    {
        var attendance = await context.CourseAttendances
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
    
    public async Task<CourseAttendanceEntity?> GetAttendanceByIdentifier(string attendanceIdentifier)
    {
        var attendance = await context.CourseAttendances
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
        return await context.CourseAttendances.AnyAsync(u => u.Id == attendanceId);
    }
    
    public async Task<bool> AttendanceCheckAvailabilityCheck(string studentCode, string attendanceIdentifier)
    {
        return await context.AttendanceChecks.AnyAsync(u => u.StudentCode == studentCode 
                                                              && u.AttendanceIdentifier == attendanceIdentifier);
    }
    
    public async Task<List<CourseAttendanceEntity>> GetCourseAttendancesByCourseId(Guid courseId, int pageNr, int pageSize)
    {
        var attendances = await context.CourseAttendances
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
    
    
    public async Task<CourseAttendanceEntity?> GetMostRecentAttendanceByUser(Guid userId)
    {
        return await context.CourseAttendances
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
        return await context.CourseAttendances
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
    
    public void SeedAttendanceTypes()
    {
        if (!context.AttendanceTypes.Any())
        {
            var now = DateTime.UtcNow;

            var attendanceTypes = new List<AttendanceTypeEntity>
            {
                new AttendanceTypeEntity
                {
                    AttendanceType = "lecture",
                    CreatedBy = "aspnet-initializer",
                    CreatedAt = now,
                    UpdatedBy = "aspnet-initializer",
                    UpdatedAt = now,
                },
                new AttendanceTypeEntity
                {
                    AttendanceType = "practice",
                    CreatedBy = "aspnet-initializer",
                    CreatedAt = now,
                    UpdatedBy = "aspnet-initializer",
                    UpdatedAt = now,
                },
                new AttendanceTypeEntity
                {
                    AttendanceType = "lecture-practice",
                    CreatedBy = "aspnet-initializer",
                    CreatedAt = now,
                    UpdatedBy = "aspnet-initializer",
                    UpdatedAt = now,
                }
            };

            context.AttendanceTypes.AddRange(attendanceTypes);
            context.SaveChanges();
        }
    }
}