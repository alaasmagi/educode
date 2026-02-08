using App.Contracts.DTOs;
using Base.DTO;

namespace App.Contracts.Services;

public interface ISchoolService
{
    Task<MethodResponse<List<SchoolDto>>> GetAllSchools(int pageNr, int pageSize);
    Task<MethodResponse<SchoolDto>> GetSchoolById(Guid id);
}