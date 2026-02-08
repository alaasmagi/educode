using App.Contracts.DTOs;
using App.Contracts.WebRequests;
using Base.DTO;

namespace App.Contracts.Services;

public interface IAttendanceCheckService
{
    Task<MethodResponse<List<AttendanceCheckDto>>> GetAttendanceChecksByAttendanceIdAsync(Guid id, int pageNr, int pageSize);
    Task<MethodResponse<AttendanceCheckDto>> GetAttendanceCheckByIdAsync(Guid id);
    Task<MethodResponse<bool>> AddAttendanceCheckAsync(AttendanceCheckRequest request, string? email, string clientApp);
    Task<MethodResponse<bool>> SoftDeleteAttendanceCheckAsync(Guid id, string email, string clientApp);
}