using Contracts.Trips;
using TripService.Core.Common;

namespace TripService.Core.Services;

public interface ITripPlannerService
{
    Task<IReadOnlyCollection<TripSummaryDto>> GetTripsAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AdminTripSummaryDto>> GetAllTripsAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<TripDto>> GetTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<TripDto>> CreateTripAsync(Guid ownerId, CreateTripRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<TripDto>> UpdateTripAsync(Guid ownerId, Guid tripId, UpdateTripRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteAnyTripAsync(Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyCollection<DestinationDto>>> GetDestinationsAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<DestinationDto>> CreateDestinationAsync(Guid ownerId, Guid tripId, CreateDestinationRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<DestinationDto>> UpdateDestinationAsync(Guid ownerId, Guid tripId, Guid destinationId, UpdateDestinationRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteDestinationAsync(Guid ownerId, Guid tripId, Guid destinationId, CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyCollection<ActivityDto>>> GetActivitiesAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<ActivityDto>> CreateActivityAsync(Guid ownerId, Guid tripId, CreateActivityRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<ActivityDto>> UpdateActivityAsync(Guid ownerId, Guid tripId, Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteActivityAsync(Guid ownerId, Guid tripId, Guid activityId, CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyCollection<ExpenseDto>>> GetExpensesAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<ExpenseDto>> CreateExpenseAsync(Guid ownerId, Guid tripId, CreateExpenseRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<ExpenseDto>> UpdateExpenseAsync(Guid ownerId, Guid tripId, Guid expenseId, UpdateExpenseRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteExpenseAsync(Guid ownerId, Guid tripId, Guid expenseId, CancellationToken cancellationToken = default);

    Task<ServiceResult<BudgetSummaryDto>> GetBudgetSummaryAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyCollection<ChecklistItemDto>>> GetChecklistItemsAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<ChecklistItemDto>> CreateChecklistItemAsync(Guid ownerId, Guid tripId, CreateChecklistItemRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<ChecklistItemDto>> UpdateChecklistItemAsync(Guid ownerId, Guid tripId, Guid itemId, UpdateChecklistItemRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteChecklistItemAsync(Guid ownerId, Guid tripId, Guid itemId, CancellationToken cancellationToken = default);
}
