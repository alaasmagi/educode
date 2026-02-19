using System.Security.Cryptography;
using System.Text;

namespace App.Infrastructure.Helpers;

public class IdentifierGenerator
{
    private const string Base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Generate(string seed)
    {
        var timestamp = DateTime.UtcNow;
        string input = $"{timestamp.Ticks}:{seed}";

        using var sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

        var chars = new char[8];
        for (int i = 0; i < 8; i++)
        {
            chars[i] = Base36[hash[i] % 36];
        }

        return $"{chars[0]}{chars[1]}{chars[2]}{chars[3]}-" +
               $"{chars[4]}{chars[5]}{chars[6]}{chars[7]}";
    }
}