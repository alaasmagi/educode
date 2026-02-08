using App.Contracts.DTOs;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface IAttendanceCheckService
{
    Task<MethodResponse<bool>> AddAttendanceCheckAsync(AttendanceCheckEntity attendanceCheck, string? workplaceIdentifier, string client);
    Task<MethodResponse<List<AttendanceCheckDto>>> GetAttendanceChecksByAttendanceIdAsync(string attendanceIdentifier, int pageNr, int pageSize);
    Task<MethodResponse<AttendanceCheckDto>> GetAttendanceCheckByIdAsync(Guid attendanceCheckId);
    Task<MethodResponse<bool>> DeleteAttendanceCheck(Guid attendanceCheckId, string email, string client);
}