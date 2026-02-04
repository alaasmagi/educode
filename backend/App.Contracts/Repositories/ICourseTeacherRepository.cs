using App.Domain.Entities;

namespace App.Contracts.Repositories;

public interface ICourseTeacherRepository : IRepository<CourseTeacherEntity>
{
    Task<List<CourseTeacherEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
    Task<List<Guid>?> GetAllIdsByTeacherAsync(Guid userId);
    Task<List<Guid>?> GetAllIdsByCourseAsync(Guid courseId);
    Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState);

}