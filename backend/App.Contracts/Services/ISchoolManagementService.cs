using App.Domain.Entities;

namespace App.Application.Contracts.Services;

public interface ISchoolManagementService
{
    Task<List<SchoolEntity>?> GetAllSchools(int pageNr, int pageSize);
    Task<SchoolEntity?> GetSchoolById(Guid id);
}