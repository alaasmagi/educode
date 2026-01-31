using App.Domain;

namespace App.DAL.Contracts;

public interface IAttendanceCheckRepository : IRepository<AttendanceCheckEntity>
{
    Task<List<AttendanceCheckEntity>?> GetAllByAttendanceAsync(Guid attendanceId);
    Task<List<AttendanceCheckEntity>?> GetAllByAttendanceIdentifierAsync(string attendanceIdentifier, int pageNr, int pageSize);
    Task<int?> GetUserCountAsync(Guid attendanceId);
    Task<List<AttendanceCheckEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}