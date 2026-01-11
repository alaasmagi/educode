using App.Domain;

namespace Contracts;

public interface ISchoolManagementService
{
    Task<List<SchoolEntity>?> GetAllSchools(int pageNr, int pageSize);
    Task<SchoolEntity?> GetSchoolById(Guid id);
}