using App.Domain;

namespace App.DAL.Contracts;

public interface IAttendanceRepository : IRepository<AttendanceEntity>
{
    Task<List<AttendanceEntity>?> GetAllByCourseAsync(Guid courseId, int pageNr, int pageSize, bool includeDeleted = false);
    Task<AttendanceEntity?> GetOngoingByUserAsync(Guid userId);
    Task<AttendanceEntity?> GetMostRecentByUserAsync(Guid userId);
    Task<List<AttendanceEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<Guid?> CheckAvailabilityByIdentifierAsync(string identifier, bool includeDeleted = false);
    void SeedAttendanceTypes(List<AttendanceTypeEntity> attendanceTypes);
}