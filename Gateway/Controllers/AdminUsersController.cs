using Contracts.Auth;
using Contracts.Common;
using Gateway.Security;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly AuthServiceHttpClient authServiceHttpClient;
    private readonly IRoleGuardService roleGuardService;

    public AdminUsersController(
        AuthServiceHttpClient authServiceHttpClient,
        IRoleGuardService roleGuardService)
    {
        this.authServiceHttpClient = authServiceHttpClient;
        this.roleGuardService = roleGuardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return await ToProxyResult(await authServiceHttpClient.GetUsersAsync(cancellationToken));
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return await ToProxyResult(await authServiceHttpClient.UpdateUserRoleAsync(userId, request, cancellationToken));
    }

    [HttpPut("{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return await ToProxyResult(await authServiceHttpClient.UpdateUserStatusAsync(userId, request, cancellationToken));
    }

    private bool IsAdmin()
    {
        return roleGuardService.TryRequireRole(UserRole.Admin, out _);
    }

    private ObjectResult Forbidden()
    {
        return StatusCode(StatusCodes.Status403Forbidden, new { error = "Admin role is required." });
    }

    private static async Task<IActionResult> ToProxyResult(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}
