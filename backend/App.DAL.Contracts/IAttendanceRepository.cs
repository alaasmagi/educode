using App.Domain;

namespace App.DAL.Contracts;

public interface IAttendanceRepository : IRepository<AttendanceEntity>
{
    Task<List<AttendanceEntity>?> SearchAsync(string keyword, bool includeDeleted = false);
}