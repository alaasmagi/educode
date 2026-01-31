using App.Domain;

namespace App.DAL.Contracts;

public interface IAttendanceTypeRepository : IRepository<AttendanceTypeEntity>
{
    Task<List<AttendanceTypeEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}