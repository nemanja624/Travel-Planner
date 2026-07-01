namespace AuthService.Core.Security;

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
