using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Contracts.Common;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AuthService.Core.Security;

public sealed class JwtAccessTokenValidator : IAccessTokenValidator
{
    private readonly JwtOptions options;

    public JwtAccessTokenValidator(JwtOptions options)
    {
        this.options = options;
    }

    public TokenValidationResult ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return TokenValidationResult.Failure("Token is missing.");
        }

        ValidateOptions();

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return TokenValidationResult.Failure("Token format is invalid.");
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Sign(unsignedToken);
        if (!FixedTimeEquals(expectedSignature, parts[2]))
        {
            return TokenValidationResult.Failure("Token signature is invalid.");
        }

        try
        {
            using var header = JsonDocument.Parse(Base64UrlDecode(parts[0])); // dekodira se header i proverava se da li je validan
            if (ReadString(header.RootElement, "alg") != "HS256" || ReadString(header.RootElement, "typ") != "JWT")
            {
                return TokenValidationResult.Failure("Token header is invalid.");
            }

            using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1])); // isto za payload
            return ValidatePayload(payload.RootElement); // validira payload i vraca rezultat
        }
        catch (JsonException)
        {
            return TokenValidationResult.Failure("Token payload is invalid.");
        }
        catch (FormatException)
        {
            return TokenValidationResult.Failure("Token encoding is invalid.");
        }
    }

    private TokenValidationResult ValidatePayload(JsonElement payload)
    {
        // proverava issuer i audience
        var issuer = ReadString(payload, "iss");
        var audience = ReadString(payload, "aud");
        if (issuer != options.Issuer || audience != options.Audience)
        {
            return TokenValidationResult.Failure("Token issuer or audience is invalid.");
        }
        // proverava vremenske oznake
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var notBefore = ReadLong(payload, "nbf");
        var expiresAt = ReadLong(payload, "exp");
        if (notBefore is null || expiresAt is null)
        {
            return TokenValidationResult.Failure("Token lifetime is invalid.");
        }

        if (now < notBefore.Value)
        {
            return TokenValidationResult.Failure("Token is not active yet.");
        }

        if (now >= expiresAt.Value)
        {
            return TokenValidationResult.Failure("Token has expired.");
        }
        // proverava korisnicke podatke 
        var subject = ReadString(payload, "sub");
        var name = ReadString(payload, "name");
        var email = ReadString(payload, "email");
        var roleValue = ReadString(payload, "role");
        if (!Guid.TryParse(subject, out var userId) ||
            string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            !Enum.TryParse<UserRole>(roleValue, out var role))
        {
            return TokenValidationResult.Failure("Token claims are invalid.");
        }
        // vraca validiran token
        return TokenValidationResult.Success(new ValidatedToken(
            userId,
            name,
            email,
            role,
            DateTimeOffset.FromUnixTimeSeconds(expiresAt.Value).UtcDateTime));
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
    }

    private string Sign(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(options.SigningKey));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static long? ReadLong(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetInt64(out var value)
            ? value
            : null;
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        return expectedBytes.Length == actualBytes.Length &&
            CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var padding = base64.Length % 4;
        if (padding > 0)
        {
            base64 = base64.PadRight(base64.Length + 4 - padding, '=');
        }

        return Convert.FromBase64String(base64);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
