using AuthService.Core.Security;

namespace Gateway.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private const string BearerPrefix = "Bearer ";
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IAccessTokenValidator accessTokenValidator;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IAccessTokenValidator accessTokenValidator)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.accessTokenValidator = accessTokenValidator;
    }

    public bool TryGetCurrentUser(out CurrentUser user)
    {
        user = default!;
        
        // extractuje authorization header
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // extractuje token iz header-a i validira ga
        var token = authorization[BearerPrefix.Length..].Trim();
        var validationResult = accessTokenValidator.ValidateToken(token); 
        if (!validationResult.Succeeded || validationResult.Token is null)
        {
            return false;
        }

        user = new CurrentUser(
            validationResult.Token.UserId,
            validationResult.Token.Name,
            validationResult.Token.Email,
            validationResult.Token.Role);

        return true;
    }
}
