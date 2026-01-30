using App.Domain;

namespace App.DAL.Contracts;

public interface IClassroomRepository : IRepository<ClassroomEntity>
{
    Task<List<ClassroomEntity>?> SearchAsync(string keyword, bool includeDeleted = false);
}