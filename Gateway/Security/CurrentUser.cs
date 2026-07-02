using Contracts.Common;

namespace Gateway.Security;

public sealed record CurrentUser(
    Guid Id,
    string Name,
    string Email,
    UserRole Role);
