using Contracts.Sharing;
using Gateway.Security;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/share-links")]
public sealed class ShareLinksController : ControllerBase
{
    private readonly TripServiceHttpClient tripServiceHttpClient;
    private readonly ICurrentUserService currentUserService;

    public ShareLinksController(
        TripServiceHttpClient tripServiceHttpClient,
        ICurrentUserService currentUserService)
    {
        this.tripServiceHttpClient = tripServiceHttpClient;
        this.currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateShareLink(CreateShareLinkRequest request, CancellationToken cancellationToken)
    {
        if (!currentUserService.TryGetCurrentUser(out var user))
        {
            return Unauthorized(new { error = "Missing or invalid access token." });
        }

        return await ToProxyResult(await tripServiceHttpClient.CreateShareLinkAsync(user.Id, request, cancellationToken));
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
