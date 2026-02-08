using Base.DTO;

namespace App.Contracts.Services;

public interface ISeedingService
{
    Task<MethodResponse<bool>> Seed();
}