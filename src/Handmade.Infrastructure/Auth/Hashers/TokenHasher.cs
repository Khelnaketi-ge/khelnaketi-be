using System.Security.Cryptography;
using System.Text;
using Handmade.Application.Interfaces;
using Handmade.Infrastructure.Options;

namespace Handmade.Infrastructure.Auth.Hashers;

public class TokenHasher(JwtOptions jwtOptions) : ITokenHasher
{
    private const string HashPrefix = "v1";

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtOptions.Secret);

        var hash = ComputeHash(token);
        return $"{HashPrefix}-{Convert.ToHexString(hash)}";
    }

    public bool VerifyToken(string token, string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(tokenHash))
            return false;

        var parts = tokenHash.Split('-', count: 2);
        
        if (parts is not [HashPrefix, _]) 
            return false;

        try
        {
            var expectedHash = Convert.FromHexString(parts[1]);
            var actualHash = ComputeHash(token);

            return expectedHash.Length == actualHash.Length
                   && CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private byte[] ComputeHash(string token)
    {
        var key = Encoding.UTF8.GetBytes(jwtOptions.Secret);
        var tokenBytes = Encoding.UTF8.GetBytes(token);

        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(tokenBytes);
    }
}
