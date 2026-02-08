using App.Contracts.DTOs;
using Base.DTO;

namespace App.Contracts.Services;

public interface ISchoolService
{
    Task<MethodResponse<List<SchoolDto>>> GetAllSchoolsAsync(int pageNr, int pageSize);
    Task<MethodResponse<SchoolDto>> GetSchoolByIdAsync(Guid id);
}