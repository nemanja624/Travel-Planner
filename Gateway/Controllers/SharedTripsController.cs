using Contracts.Sharing;
using Microsoft.AspNetCore.Mvc;
using TripService.Core.Services;

namespace Gateway.Controllers;

[ApiController]
[Route("api/shared-trips")]
public sealed class SharedTripsController : ControllerBase
{
    private readonly IShareLinkService shareLinkService;

    public SharedTripsController(IShareLinkService shareLinkService)
    {
        this.shareLinkService = shareLinkService;
    }

    [HttpGet("{token}")]
    public async Task<ActionResult<SharedTripDto>> GetSharedTrip(string token, CancellationToken cancellationToken)
    {
        var result = await shareLinkService.GetSharedTripAsync(token, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
