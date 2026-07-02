using Contracts.Trips;
using Gateway.Security;
using Microsoft.AspNetCore.Mvc;
using TripService.Core.Common;
using TripService.Core.Services;

namespace Gateway.Controllers;

[ApiController]
[Route("api/trips")]
public sealed class TripsController : ControllerBase
{
    private readonly ITripPlannerService tripPlannerService;
    private readonly ICurrentUserService currentUserService;

    public TripsController(
        ITripPlannerService tripPlannerService,
        ICurrentUserService currentUserService)
    {
        this.tripPlannerService = tripPlannerService;
        this.currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TripSummaryDto>>> GetTrips(CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        var trips = await tripPlannerService.GetTripsAsync(ownerId, cancellationToken);
        return Ok(trips);
    }

    [HttpGet("{tripId:guid}")]
    public async Task<ActionResult<TripDto>> GetTrip(Guid tripId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.GetTripAsync(ownerId, tripId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<TripDto>> CreateTrip(CreateTripRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        var result = await tripPlannerService.CreateTripAsync(ownerId, request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetTrip), new { tripId = result.Value.Id }, result.Value);
    }

    [HttpPut("{tripId:guid}")]
    public async Task<ActionResult<TripDto>> UpdateTrip(Guid tripId, UpdateTripRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.UpdateTripAsync(ownerId, tripId, request, cancellationToken));
    }

    [HttpDelete("{tripId:guid}")]
    public async Task<IActionResult> DeleteTrip(Guid tripId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.DeleteTripAsync(ownerId, tripId, cancellationToken));
    }

    [HttpGet("{tripId:guid}/destinations")]
    public async Task<ActionResult<IReadOnlyCollection<DestinationDto>>> GetDestinations(Guid tripId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.GetDestinationsAsync(ownerId, tripId, cancellationToken));
    }

    [HttpPost("{tripId:guid}/destinations")]
    public async Task<ActionResult<DestinationDto>> CreateDestination(Guid tripId, CreateDestinationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.CreateDestinationAsync(ownerId, tripId, request, cancellationToken));
    }

    [HttpPut("{tripId:guid}/destinations/{destinationId:guid}")]
    public async Task<ActionResult<DestinationDto>> UpdateDestination(Guid tripId, Guid destinationId, UpdateDestinationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.UpdateDestinationAsync(ownerId, tripId, destinationId, request, cancellationToken));
    }

    [HttpDelete("{tripId:guid}/destinations/{destinationId:guid}")]
    public async Task<IActionResult> DeleteDestination(Guid tripId, Guid destinationId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.DeleteDestinationAsync(ownerId, tripId, destinationId, cancellationToken));
    }

    [HttpGet("{tripId:guid}/activities")]
    public async Task<ActionResult<IReadOnlyCollection<ActivityDto>>> GetActivities(Guid tripId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.GetActivitiesAsync(ownerId, tripId, cancellationToken));
    }

    [HttpPost("{tripId:guid}/activities")]
    public async Task<ActionResult<ActivityDto>> CreateActivity(Guid tripId, CreateActivityRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.CreateActivityAsync(ownerId, tripId, request, cancellationToken));
    }

    [HttpPut("{tripId:guid}/activities/{activityId:guid}")]
    public async Task<ActionResult<ActivityDto>> UpdateActivity(Guid tripId, Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.UpdateActivityAsync(ownerId, tripId, activityId, request, cancellationToken));
    }

    [HttpDelete("{tripId:guid}/activities/{activityId:guid}")]
    public async Task<IActionResult> DeleteActivity(Guid tripId, Guid activityId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.DeleteActivityAsync(ownerId, tripId, activityId, cancellationToken));
    }

    [HttpGet("{tripId:guid}/expenses")]
    public async Task<ActionResult<IReadOnlyCollection<ExpenseDto>>> GetExpenses(Guid tripId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.GetExpensesAsync(ownerId, tripId, cancellationToken));
    }

    [HttpPost("{tripId:guid}/expenses")]
    public async Task<ActionResult<ExpenseDto>> CreateExpense(Guid tripId, CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.CreateExpenseAsync(ownerId, tripId, request, cancellationToken));
    }

    [HttpPut("{tripId:guid}/expenses/{expenseId:guid}")]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(Guid tripId, Guid expenseId, UpdateExpenseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.UpdateExpenseAsync(ownerId, tripId, expenseId, request, cancellationToken));
    }

    [HttpDelete("{tripId:guid}/expenses/{expenseId:guid}")]
    public async Task<IActionResult> DeleteExpense(Guid tripId, Guid expenseId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.DeleteExpenseAsync(ownerId, tripId, expenseId, cancellationToken));
    }

    [HttpGet("{tripId:guid}/budget")]
    public async Task<ActionResult<BudgetSummaryDto>> GetBudget(Guid tripId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.GetBudgetSummaryAsync(ownerId, tripId, cancellationToken));
    }

    [HttpGet("{tripId:guid}/checklist-items")]
    public async Task<ActionResult<IReadOnlyCollection<ChecklistItemDto>>> GetChecklistItems(Guid tripId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.GetChecklistItemsAsync(ownerId, tripId, cancellationToken));
    }

    [HttpPost("{tripId:guid}/checklist-items")]
    public async Task<ActionResult<ChecklistItemDto>> CreateChecklistItem(Guid tripId, CreateChecklistItemRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.CreateChecklistItemAsync(ownerId, tripId, request, cancellationToken));
    }

    [HttpPut("{tripId:guid}/checklist-items/{itemId:guid}")]
    public async Task<ActionResult<ChecklistItemDto>> UpdateChecklistItem(Guid tripId, Guid itemId, UpdateChecklistItemRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.UpdateChecklistItemAsync(ownerId, tripId, itemId, request, cancellationToken));
    }

    [HttpDelete("{tripId:guid}/checklist-items/{itemId:guid}")]
    public async Task<IActionResult> DeleteChecklistItem(Guid tripId, Guid itemId, CancellationToken cancellationToken)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return MissingUser();
        }

        return ToActionResult(await tripPlannerService.DeleteChecklistItemAsync(ownerId, tripId, itemId, cancellationToken));
    }

    private bool TryGetOwnerId(out Guid ownerId)
    {
        ownerId = Guid.Empty;

        if (!currentUserService.TryGetCurrentUser(out var user))
        {
            return false;
        }

        ownerId = user.Id;
        return true;
    }

    private UnauthorizedObjectResult MissingUser()
    {
        return Unauthorized(new { error = "Missing or invalid access token." });
    }

    private ActionResult<T> ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Ok(result.Value);
        }

        return IsNotFound(result.Error)
            ? NotFound(new { error = result.Error }) // 404 Not Found
            : BadRequest(new { error = result.Error }); // 400 Bad Request
    }

    private IActionResult ToActionResult(ServiceResult result)
    {
        if (result.Succeeded)
        {
            return NoContent();
        }

        return IsNotFound(result.Error)
            ? NotFound(new { error = result.Error })
            : BadRequest(new { error = result.Error });
    }

    private static bool IsNotFound(string? error)
    {
        return error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true;
    }
}
