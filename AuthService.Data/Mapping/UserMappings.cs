using AuthService.Data.Models;
using Contracts.Auth;

namespace AuthService.Data.Mapping;

public static class UserMappings
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc);
    }
}
