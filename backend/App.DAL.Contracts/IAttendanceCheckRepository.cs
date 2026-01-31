using App.Domain;

namespace App.DAL.Contracts;

public interface IAttendanceCheckRepository : IRepository<AttendanceCheckEntity>
{
    Task<List<AttendanceCheckEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}