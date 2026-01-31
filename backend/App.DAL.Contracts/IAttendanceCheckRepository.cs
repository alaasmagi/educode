using App.Domain;

namespace App.DAL.Contracts;

public interface IAttendanceCheckRepository : IRepository<AttendanceCheckEntity>
{
    Task<List<AttendanceCheckEntity>?> GetAllByAttendanceAysnc(Guid attendanceId);
    Task<List<AttendanceCheckEntity>?> GetAllByAttendanceIdentifierAsync(string attendanceIdentifier, int pageNr, int pageSize);
    Task<int?> GetUserCountsAsync(Guid attendanceId);
    Task<List<AttendanceCheckEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}