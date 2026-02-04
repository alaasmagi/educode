using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IAttendanceRepository : IRepository<AttendanceEntity>
{
    Task<List<AttendanceEntity>?> GetAllByCourseAsync(Guid courseId, int pageNr, int pageSize, bool includeDeleted = false);
    Task<AttendanceEntity?> GetOngoingByUserAsync(Guid userId);
    Task<AttendanceEntity?> GetMostRecentByUserAsync(Guid userId);
    Task<List<AttendanceEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByIdentifierAsync(string identifier, bool includeDeleted = false);
    Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState);
}