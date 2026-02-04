using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface IAttendanceTypeRepository : IRepository<AttendanceTypeEntity>
{
    Task<List<AttendanceTypeEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}