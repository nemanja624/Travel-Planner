namespace AuthService.Core.Security;

public sealed record TokenValidationResult(bool Succeeded, ValidatedToken? Token, string? Error)
{
    public static TokenValidationResult Success(ValidatedToken token)
    {
        return new TokenValidationResult(true, token, null);
    }

    public static TokenValidationResult Failure(string error)
    {
        return new TokenValidationResult(false, null, error);
    }
}
