namespace App.Contracts.Services;

public interface IPasswordService
{
    Task<string> HashPasswordAsync(string input);
    Task<bool> VerifyPasswordAsync(string hashedPassword, string input);
}