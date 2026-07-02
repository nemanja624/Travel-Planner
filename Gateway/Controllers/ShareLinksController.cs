using Contracts.Sharing;
using Gateway.Security;
using Microsoft.AspNetCore.Mvc;
using TripService.Core.Common;
using TripService.Core.Services;

namespace Gateway.Controllers;

[ApiController]
[Route("api/share-links")]
public sealed class ShareLinksController : ControllerBase
{
    private readonly IShareLinkService shareLinkService;
    private readonly ICurrentUserService currentUserService;

    public ShareLinksController(
        IShareLinkService shareLinkService,
        ICurrentUserService currentUserService)
    {
        this.shareLinkService = shareLinkService;
        this.currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<ShareLinkDto>> CreateShareLink(CreateShareLinkRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserService.TryGetCurrentUser(out var user))
        {
            return Unauthorized(new { error = "Missing or invalid access token." });
        }

        var result = await shareLinkService.CreateShareLinkAsync(user.Id, request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return IsNotFound(result.Error)
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    private static bool IsNotFound(string? error)
    {
        return error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true;
    }
}
