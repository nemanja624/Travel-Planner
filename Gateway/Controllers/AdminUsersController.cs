using AuthService.Core.Common;
using AuthService.Core.Services;
using Contracts.Auth;
using Contracts.Common;
using Gateway.Security;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService adminUserService;
    private readonly IRoleGuardService roleGuardService;

    public AdminUsersController(
        IAdminUserService adminUserService,
        IRoleGuardService roleGuardService)
    {
        this.adminUserService = adminUserService;
        this.roleGuardService = roleGuardService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return Ok(await adminUserService.GetUsersAsync(cancellationToken));
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<ActionResult<UserDto>> UpdateUserRole(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return ToActionResult(await adminUserService.UpdateUserRoleAsync(userId, request.Role, cancellationToken));
    }

    [HttpPut("{userId:guid}/status")]
    public async Task<ActionResult<UserDto>> UpdateUserStatus(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return ToActionResult(await adminUserService.UpdateUserStatusAsync(userId, request.IsActive, cancellationToken));
    }

    private bool IsAdmin()
    {
        return roleGuardService.TryRequireRole(UserRole.Admin, out _);
    }

    private ObjectResult Forbidden()
    {
        return StatusCode(StatusCodes.Status403Forbidden, new { error = "Admin role is required." });
    }

    private ActionResult<UserDto> ToActionResult(ServiceResult<UserDto> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(result.Value);
        }

        return NotFound(new { error = result.Error });
    }
}
