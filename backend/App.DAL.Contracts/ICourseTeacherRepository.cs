using App.Domain;

namespace App.DAL.Contracts;

public interface ICourseTeacherRepository : IRepository<CourseTeacherEntity>
{
    Task<List<CourseTeacherEntity>?> SearchAsync(string keyword, Guid? resourceFilterId = null, bool includeDeleted = false);
}