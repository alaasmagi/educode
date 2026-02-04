using System.Security.Cryptography;
using System.Text;
using App.Contracts.Services;
using Konscious.Security.Cryptography;

namespace App.Infrastructure.Argon2;

public class ArgonService : IPasswordService
{
    private static readonly SemaphoreSlim Argon2Semaphore = new(4, 4);

    public async Task<string> HashPasswordAsync(string input)
    {
        await Argon2Semaphore.WaitAsync();
        try
        {
            var salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(input))
            {
                Salt = salt,
                DegreeOfParallelism = 1,
                Iterations = 2,
                MemorySize = 16 * 1024
            };

            var hash = await argon2.GetBytesAsync(32);

            var result = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, result, salt.Length, hash.Length);

            return Convert.ToBase64String(result);
        }
        finally
        {
            Argon2Semaphore.Release();
        }
    }

    public async Task<bool> VerifyPasswordAsync(string input, string storedHash)
    {
        await Argon2Semaphore.WaitAsync();
        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);

            var salt = new byte[16];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(input))
            {
                Salt = salt,
                DegreeOfParallelism = 1,
                Iterations = 2,
                MemorySize = 16 * 1024
            };

            var newHash = await argon2.GetBytesAsync(32);

            return CryptographicOperations.FixedTimeEquals(
                hashBytes.AsSpan(16, 32), 
                newHash
            );
        }
        finally
        {
            Argon2Semaphore.Release();
        }
    }
}