using App.Contracts.DTOs;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using Base.DTO;
using Microsoft.AspNetCore.Http;

namespace App.Contracts.Services;

public interface IUserService
{
    Task<MethodResponse<List<UserDto>>> GetAllUsersAsync(int pageNr, int pageSize);
    Task<MethodResponse<UserDto>> GetUserByIdAsync(Guid id);
    Task<MethodResponse<bool>> UpdateUserAsync(UserRequest request, string email, string clientApp);
    Task<MethodResponse<bool>> SoftDeleteUserAsync(Guid userId, string email, string clientApp);
    Task<MethodResponse<bool>> RestoreUserAsync(Guid userId, string email, string clientApp);
    Task<MethodResponse<bool>> UploadPhotoAsync(Guid userId, IFormFile image, string email, string clientApp);
    Task<MethodResponse<bool>> RemovePhotoAsync(Guid userId, string email, string clientApp);
}

