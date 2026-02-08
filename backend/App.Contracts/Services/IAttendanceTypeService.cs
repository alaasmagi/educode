using App.Contracts.DTOs;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface IAttendanceTypeService
{
    Task<MethodResponse<List<AttendanceTypeDto>>> GetAttendanceTypesAsync();
    Task<MethodResponse<AttendanceTypeDto>> GetAttendanceTypeByIdAsync(Guid attendanceTypeId);
}