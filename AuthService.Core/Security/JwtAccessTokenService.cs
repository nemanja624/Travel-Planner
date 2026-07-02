using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AuthService.Data.Models;

namespace AuthService.Core.Security;

public sealed class JwtAccessTokenService : IAccessTokenService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly JwtOptions options;

    public JwtAccessTokenService(JwtOptions options)
    {
        this.options = options;
    }

    public AccessTokenResult CreateToken(User user)
    {
        ValidateOptions();

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(options.ExpirationMinutes);
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256", // algoritam za potpisivanje tokena (HMAC-SHA256)
            ["typ"] = "JWT" // tip tokena 
        };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = options.Issuer, // issuer
            ["aud"] = options.Audience, // audience
            ["sub"] = user.Id.ToString(), // subject
            ["name"] = user.Name, 
            ["email"] = user.Email,
            ["role"] = user.Role.ToString(),
            ["jti"] = Guid.NewGuid().ToString(), // jedinstveni id tokena
            ["iat"] = now.ToUnixTimeSeconds(), // issued at
            ["nbf"] = now.ToUnixTimeSeconds(), // not before
            ["exp"] = expiresAt.ToUnixTimeSeconds() 
        };

        var encodedHeader = EncodeJson(header);
        var encodedPayload = EncodeJson(payload);
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";
        var signature = Sign(unsignedToken);

        return new AccessTokenResult($"{unsignedToken}.{signature}", expiresAt.UtcDateTime);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey) || Encoding.UTF8.GetByteCount(options.SigningKey) < 32)
        {
            throw new InvalidOperationException("JWT signing key must contain at least 32 bytes.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT expiration must be greater than zero.");
        }
    }

    private string Sign(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.SigningKey));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static string EncodeJson<T>(T value)
    {
        return Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
