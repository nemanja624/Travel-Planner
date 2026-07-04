using Contracts.Trips;
using Gateway.Security;
using Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[ApiController]
[Route("api/trips")]
public sealed class TripsController : ControllerBase
{
    private readonly TripServiceHttpClient tripServiceHttpClient;
    private readonly ICurrentUserService currentUserService;

    public TripsController(
        TripServiceHttpClient tripServiceHttpClient,
        ICurrentUserService currentUserService)
    {
        this.tripServiceHttpClient = tripServiceHttpClient;
        this.currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips(CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.GetTripsAsync(ownerId, cancellationToken));

    [HttpGet("{tripId:guid}")]
    public async Task<IActionResult> GetTrip(Guid tripId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.GetTripAsync(ownerId, tripId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> CreateTrip(CreateTripRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.CreateTripAsync(ownerId, request, cancellationToken));

    [HttpPut("{tripId:guid}")]
    public async Task<IActionResult> UpdateTrip(Guid tripId, UpdateTripRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.UpdateTripAsync(ownerId, tripId, request, cancellationToken));

    [HttpDelete("{tripId:guid}")]
    public async Task<IActionResult> DeleteTrip(Guid tripId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.DeleteTripAsync(ownerId, tripId, cancellationToken));

    [HttpGet("{tripId:guid}/destinations")]
    public async Task<IActionResult> GetDestinations(Guid tripId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.GetDestinationsAsync(ownerId, tripId, cancellationToken));

    [HttpPost("{tripId:guid}/destinations")]
    public async Task<IActionResult> CreateDestination(Guid tripId, CreateDestinationRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.CreateDestinationAsync(ownerId, tripId, request, cancellationToken));

    [HttpPut("{tripId:guid}/destinations/{destinationId:guid}")]
    public async Task<IActionResult> UpdateDestination(Guid tripId, Guid destinationId, UpdateDestinationRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.UpdateDestinationAsync(ownerId, tripId, destinationId, request, cancellationToken));

    [HttpDelete("{tripId:guid}/destinations/{destinationId:guid}")]
    public async Task<IActionResult> DeleteDestination(Guid tripId, Guid destinationId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.DeleteDestinationAsync(ownerId, tripId, destinationId, cancellationToken));

    [HttpGet("{tripId:guid}/activities")]
    public async Task<IActionResult> GetActivities(Guid tripId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.GetActivitiesAsync(ownerId, tripId, cancellationToken));

    [HttpPost("{tripId:guid}/activities")]
    public async Task<IActionResult> CreateActivity(Guid tripId, CreateActivityRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.CreateActivityAsync(ownerId, tripId, request, cancellationToken));

    [HttpPut("{tripId:guid}/activities/{activityId:guid}")]
    public async Task<IActionResult> UpdateActivity(Guid tripId, Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.UpdateActivityAsync(ownerId, tripId, activityId, request, cancellationToken));

    [HttpDelete("{tripId:guid}/activities/{activityId:guid}")]
    public async Task<IActionResult> DeleteActivity(Guid tripId, Guid activityId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.DeleteActivityAsync(ownerId, tripId, activityId, cancellationToken));

    [HttpGet("{tripId:guid}/expenses")]
    public async Task<IActionResult> GetExpenses(Guid tripId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.GetExpensesAsync(ownerId, tripId, cancellationToken));

    [HttpPost("{tripId:guid}/expenses")]
    public async Task<IActionResult> CreateExpense(Guid tripId, CreateExpenseRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.CreateExpenseAsync(ownerId, tripId, request, cancellationToken));

    [HttpPut("{tripId:guid}/expenses/{expenseId:guid}")]
    public async Task<IActionResult> UpdateExpense(Guid tripId, Guid expenseId, UpdateExpenseRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.UpdateExpenseAsync(ownerId, tripId, expenseId, request, cancellationToken));

    [HttpDelete("{tripId:guid}/expenses/{expenseId:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid tripId, Guid expenseId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.DeleteExpenseAsync(ownerId, tripId, expenseId, cancellationToken));

    [HttpGet("{tripId:guid}/budget")]
    public async Task<IActionResult> GetBudget(Guid tripId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.GetBudgetAsync(ownerId, tripId, cancellationToken));

    [HttpGet("{tripId:guid}/checklist-items")]
    public async Task<IActionResult> GetChecklistItems(Guid tripId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.GetChecklistItemsAsync(ownerId, tripId, cancellationToken));

    [HttpPost("{tripId:guid}/checklist-items")]
    public async Task<IActionResult> CreateChecklistItem(Guid tripId, CreateChecklistItemRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.CreateChecklistItemAsync(ownerId, tripId, request, cancellationToken));

    [HttpPut("{tripId:guid}/checklist-items/{itemId:guid}")]
    public async Task<IActionResult> UpdateChecklistItem(Guid tripId, Guid itemId, UpdateChecklistItemRequest request, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.UpdateChecklistItemAsync(ownerId, tripId, itemId, request, cancellationToken));

    [HttpDelete("{tripId:guid}/checklist-items/{itemId:guid}")]
    public async Task<IActionResult> DeleteChecklistItem(Guid tripId, Guid itemId, CancellationToken cancellationToken) =>
        await WithOwner(ownerId => tripServiceHttpClient.DeleteChecklistItemAsync(ownerId, tripId, itemId, cancellationToken));

    private async Task<IActionResult> WithOwner(Func<Guid, Task<HttpResponseMessage>> requestFactory)
    {
        if (!currentUserService.TryGetCurrentUser(out var user))
        {
            return Unauthorized(new { error = "Missing or invalid access token." });
        }

        return await ToProxyResult(await requestFactory(user.Id));
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
