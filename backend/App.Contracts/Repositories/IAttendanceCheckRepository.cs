using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IAttendanceCheckRepository : IRepository<AttendanceCheckEntity>
{
    Task<List<AttendanceCheckEntity>?> GetAllByAttendanceAsync(Guid attendanceId);
    Task<List<AttendanceCheckEntity>?> GetAllByAttendanceIdentifierAsync(string attendanceIdentifier, int pageNr, int pageSize);
    Task<List<Guid>?> GetAllIdsByUserFullNameAsync(string fullName);
    Task<int?> GetUserCountAsync(Guid attendanceId);
    Task<List<AttendanceCheckEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState);
}