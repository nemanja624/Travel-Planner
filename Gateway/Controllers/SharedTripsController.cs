using Contracts.Trips;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/shared-trips")]
public sealed class SharedTripsController : ControllerBase
{
    private readonly TripServiceHttpClient tripServiceHttpClient;

    public SharedTripsController(TripServiceHttpClient tripServiceHttpClient)
    {
        this.tripServiceHttpClient = tripServiceHttpClient;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetSharedTrip(string token, CancellationToken cancellationToken)
    {
        return await ToProxyResult(await tripServiceHttpClient.GetSharedTripAsync(token, cancellationToken));
    }

    [HttpPut("{token}/trip")]
    public async Task<IActionResult> UpdateSharedTrip(string token, UpdateTripRequest request, CancellationToken cancellationToken)
    {
        return await ToProxyResult(await tripServiceHttpClient.UpdateSharedTripAsync(token, request, cancellationToken));
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
