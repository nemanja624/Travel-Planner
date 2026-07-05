using Contracts.Common;
using Gateway.Security;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/admin/trips")]
public sealed class AdminTripsController : ControllerBase
{
    private readonly TripServiceHttpClient tripServiceHttpClient;
    private readonly IRoleGuardService roleGuardService;

    public AdminTripsController(
        TripServiceHttpClient tripServiceHttpClient,
        IRoleGuardService roleGuardService)
    {
        this.tripServiceHttpClient = tripServiceHttpClient;
        this.roleGuardService = roleGuardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips(CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return await ToProxyResult(await tripServiceHttpClient.GetAdminTripsAsync(cancellationToken));
    }

    [HttpDelete("{tripId:guid}")]
    public async Task<IActionResult> DeleteTrip(Guid tripId, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbidden();
        }

        return await ToProxyResult(await tripServiceHttpClient.DeleteAdminTripAsync(tripId, cancellationToken));
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
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        var content = await response.Content.ReadAsStringAsync();
        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}
