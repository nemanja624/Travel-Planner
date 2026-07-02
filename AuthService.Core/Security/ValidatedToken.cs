using Contracts.Common;

namespace AuthService.Core.Security;

public sealed record ValidatedToken(
    Guid UserId,
    string Name,
    string Email,
    UserRole Role,
    DateTime ExpiresAtUtc);
