using Contracts.Sharing;
using Contracts.Trips;
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

    [HttpPut("{token}/trip")]
    public async Task<ActionResult<TripDto>> UpdateSharedTrip(string token, UpdateTripRequest request, CancellationToken cancellationToken)
    {
        var result = await shareLinkService.UpdateSharedTripAsync(token, request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return result.Error?.Contains("does not allow editing", StringComparison.OrdinalIgnoreCase) == true
                ? StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error }) // ako greska sadrzi "does not allow editing", vrati 403 Forbidden
                : BadRequest(new { error = result.Error }); // ako ne sadrzi onda vrati 400 Bad Request
        }

        return Ok(result.Value);
    }
}
