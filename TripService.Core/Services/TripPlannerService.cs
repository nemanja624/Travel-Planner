using Contracts.Trips;
using Microsoft.EntityFrameworkCore;
using TripService.Core.Common;
using TripService.Data;
using TripService.Data.Mapping;
using TripService.Data.Models;

namespace TripService.Core.Services;

public sealed class TripPlannerService : ITripPlannerService
{
    private readonly TripDbContext dbContext;

    public TripPlannerService(TripDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TripSummaryDto>> GetTripsAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        var trips = await dbContext.Trips
            .AsNoTracking()
            .Include(trip => trip.Expenses)
            .Where(trip => trip.OwnerId == ownerId)
            .OrderBy(trip => trip.StartDate)
            .ToListAsync(cancellationToken);

        return trips.Select(trip => trip.ToSummaryDto()).ToList();
    }

    public async Task<ServiceResult<TripDto>> GetTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await FindOwnedTrip(ownerId, tripId)
            .AsNoTracking()
            .Include(trip => trip.Expenses)
            .FirstOrDefaultAsync(cancellationToken);

        if (trip is null)
        {
            return ServiceResult<TripDto>.Failure("Trip was not found.");
        }

        return ServiceResult<TripDto>.Success(trip.ToDto());
    }

    public async Task<ServiceResult<TripDto>> CreateTripAsync(Guid ownerId, CreateTripRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateTripDatesAndBudget(request.StartDate, request.EndDate, request.PlannedBudget);
        if (validationError is not null)
        {
            return ServiceResult<TripDto>.Failure(validationError);
        }

        var trip = request.ToEntity(ownerId);

        await dbContext.Trips.AddAsync(trip, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<TripDto>.Success(trip.ToDto());
    }

    public async Task<ServiceResult<TripDto>> UpdateTripAsync(Guid ownerId, Guid tripId, UpdateTripRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateTripDatesAndBudget(request.StartDate, request.EndDate, request.PlannedBudget);
        if (validationError is not null)
        {
            return ServiceResult<TripDto>.Failure(validationError);
        }

        var trip = await FindOwnedTrip(ownerId, tripId)
            .Include(trip => trip.Expenses)
            .FirstOrDefaultAsync(cancellationToken);

        if (trip is null)
        {
            return ServiceResult<TripDto>.Failure("Trip was not found.");
        }

        request.Apply(trip);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<TripDto>.Success(trip.ToDto());
    }

    public async Task<ServiceResult> DeleteTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await FindOwnedTrip(ownerId, tripId)
            .FirstOrDefaultAsync(cancellationToken);

        if (trip is null)
        {
            return ServiceResult.Failure("Trip was not found.");
        }

        dbContext.Trips.Remove(trip);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyCollection<DestinationDto>>> GetDestinationsAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<IReadOnlyCollection<DestinationDto>>.Failure("Trip was not found.");
        }

        var destinations = await dbContext.Destinations
            .AsNoTracking()
            .Where(destination => destination.TripId == tripId)
            .OrderBy(destination => destination.ArrivalDate)
            .ThenBy(destination => destination.Name)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<DestinationDto>>.Success(destinations.Select(destination => destination.ToDto()).ToList());
    }

    public async Task<ServiceResult<DestinationDto>> CreateDestinationAsync(Guid ownerId, Guid tripId, CreateDestinationRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateDestinationDates(request.ArrivalDate, request.DepartureDate);
        if (validationError is not null)
        {
            return ServiceResult<DestinationDto>.Failure(validationError);
        }

        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<DestinationDto>.Failure("Trip was not found.");
        }

        var destination = request.ToEntity(tripId);
        await dbContext.Destinations.AddAsync(destination, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<DestinationDto>.Success(destination.ToDto());
    }

    public async Task<ServiceResult<DestinationDto>> UpdateDestinationAsync(Guid ownerId, Guid tripId, Guid destinationId, UpdateDestinationRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateDestinationDates(request.ArrivalDate, request.DepartureDate);
        if (validationError is not null)
        {
            return ServiceResult<DestinationDto>.Failure(validationError);
        }

        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<DestinationDto>.Failure("Trip was not found.");
        }

        var destination = await dbContext.Destinations
            .FirstOrDefaultAsync(destination => destination.TripId == tripId && destination.Id == destinationId, cancellationToken);

        if (destination is null)
        {
            return ServiceResult<DestinationDto>.Failure("Destination was not found.");
        }

        request.Apply(destination);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<DestinationDto>.Success(destination.ToDto());
    }

    public async Task<ServiceResult> DeleteDestinationAsync(Guid ownerId, Guid tripId, Guid destinationId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult.Failure("Trip was not found.");
        }

        var destination = await dbContext.Destinations
            .FirstOrDefaultAsync(destination => destination.TripId == tripId && destination.Id == destinationId, cancellationToken);

        if (destination is null)
        {
            return ServiceResult.Failure("Destination was not found.");
        }

        dbContext.Destinations.Remove(destination);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyCollection<ActivityDto>>> GetActivitiesAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<IReadOnlyCollection<ActivityDto>>.Failure("Trip was not found.");
        }

        var activities = await dbContext.Activities
            .AsNoTracking()
            .Where(activity => activity.TripId == tripId)
            .OrderBy(activity => activity.Date)
            .ThenBy(activity => activity.Time)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<ActivityDto>>.Success(activities.Select(activity => activity.ToDto()).ToList());
    }

    public async Task<ServiceResult<ActivityDto>> CreateActivityAsync(Guid ownerId, Guid tripId, CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateActivity(request.EstimatedCost);
        if (validationError is not null)
        {
            return ServiceResult<ActivityDto>.Failure(validationError);
        }

        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<ActivityDto>.Failure("Trip was not found.");
        }

        var activity = request.ToEntity(tripId);
        await dbContext.Activities.AddAsync(activity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ActivityDto>.Success(activity.ToDto());
    }

    public async Task<ServiceResult<ActivityDto>> UpdateActivityAsync(Guid ownerId, Guid tripId, Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateActivity(request.EstimatedCost);
        if (validationError is not null)
        {
            return ServiceResult<ActivityDto>.Failure(validationError);
        }

        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<ActivityDto>.Failure("Trip was not found.");
        }

        var activity = await dbContext.Activities
            .FirstOrDefaultAsync(activity => activity.TripId == tripId && activity.Id == activityId, cancellationToken);

        if (activity is null)
        {
            return ServiceResult<ActivityDto>.Failure("Activity was not found.");
        }

        request.Apply(activity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ActivityDto>.Success(activity.ToDto());
    }

    public async Task<ServiceResult> DeleteActivityAsync(Guid ownerId, Guid tripId, Guid activityId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult.Failure("Trip was not found.");
        }

        var activity = await dbContext.Activities
            .FirstOrDefaultAsync(activity => activity.TripId == tripId && activity.Id == activityId, cancellationToken);

        if (activity is null)
        {
            return ServiceResult.Failure("Activity was not found.");
        }

        dbContext.Activities.Remove(activity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyCollection<ExpenseDto>>> GetExpensesAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<IReadOnlyCollection<ExpenseDto>>.Failure("Trip was not found.");
        }

        var expenses = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.TripId == tripId)
            .OrderBy(expense => expense.Date)
            .ThenBy(expense => expense.Name)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<ExpenseDto>>.Success(expenses.Select(expense => expense.ToDto()).ToList());
    }

    public async Task<ServiceResult<ExpenseDto>> CreateExpenseAsync(Guid ownerId, Guid tripId, CreateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateExpense(request.Amount);
        if (validationError is not null)
        {
            return ServiceResult<ExpenseDto>.Failure(validationError);
        }

        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<ExpenseDto>.Failure("Trip was not found.");
        }

        var expense = request.ToEntity(tripId);
        await dbContext.Expenses.AddAsync(expense, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ExpenseDto>.Success(expense.ToDto());
    }

    public async Task<ServiceResult<ExpenseDto>> UpdateExpenseAsync(Guid ownerId, Guid tripId, Guid expenseId, UpdateExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateExpense(request.Amount);
        if (validationError is not null)
        {
            return ServiceResult<ExpenseDto>.Failure(validationError);
        }

        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<ExpenseDto>.Failure("Trip was not found.");
        }

        var expense = await dbContext.Expenses
            .FirstOrDefaultAsync(expense => expense.TripId == tripId && expense.Id == expenseId, cancellationToken);

        if (expense is null)
        {
            return ServiceResult<ExpenseDto>.Failure("Expense was not found.");
        }

        request.Apply(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ExpenseDto>.Success(expense.ToDto());
    }

    public async Task<ServiceResult> DeleteExpenseAsync(Guid ownerId, Guid tripId, Guid expenseId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult.Failure("Trip was not found.");
        }

        var expense = await dbContext.Expenses
            .FirstOrDefaultAsync(expense => expense.TripId == tripId && expense.Id == expenseId, cancellationToken);

        if (expense is null)
        {
            return ServiceResult.Failure("Expense was not found.");
        }

        dbContext.Expenses.Remove(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<BudgetSummaryDto>> GetBudgetSummaryAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await FindOwnedTrip(ownerId, tripId)
            .AsNoTracking()
            .Include(trip => trip.Expenses)
            .FirstOrDefaultAsync(cancellationToken);

        if (trip is null)
        {
            return ServiceResult<BudgetSummaryDto>.Failure("Trip was not found.");
        }

        return ServiceResult<BudgetSummaryDto>.Success(trip.ToBudgetSummaryDto());
    }

    public async Task<ServiceResult<IReadOnlyCollection<ChecklistItemDto>>> GetChecklistItemsAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<IReadOnlyCollection<ChecklistItemDto>>.Failure("Trip was not found.");
        }

        var items = await dbContext.ChecklistItems
            .AsNoTracking()
            .Where(item => item.TripId == tripId)
            .OrderBy(item => item.IsCompleted)
            .ThenBy(item => item.Text)
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<ChecklistItemDto>>.Success(items.Select(item => item.ToDto()).ToList());
    }

    public async Task<ServiceResult<ChecklistItemDto>> CreateChecklistItemAsync(Guid ownerId, Guid tripId, CreateChecklistItemRequest request, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<ChecklistItemDto>.Failure("Trip was not found.");
        }

        var item = request.ToEntity(tripId);
        await dbContext.ChecklistItems.AddAsync(item, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ChecklistItemDto>.Success(item.ToDto());
    }

    public async Task<ServiceResult<ChecklistItemDto>> UpdateChecklistItemAsync(Guid ownerId, Guid tripId, Guid itemId, UpdateChecklistItemRequest request, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult<ChecklistItemDto>.Failure("Trip was not found.");
        }

        var item = await dbContext.ChecklistItems
            .FirstOrDefaultAsync(item => item.TripId == tripId && item.Id == itemId, cancellationToken);

        if (item is null)
        {
            return ServiceResult<ChecklistItemDto>.Failure("Checklist item was not found.");
        }

        request.Apply(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ChecklistItemDto>.Success(item.ToDto());
    }

    public async Task<ServiceResult> DeleteChecklistItemAsync(Guid ownerId, Guid tripId, Guid itemId, CancellationToken cancellationToken = default)
    {
        if (!await TripExistsAsync(ownerId, tripId, cancellationToken))
        {
            return ServiceResult.Failure("Trip was not found.");
        }

        var item = await dbContext.ChecklistItems
            .FirstOrDefaultAsync(item => item.TripId == tripId && item.Id == itemId, cancellationToken);

        if (item is null)
        {
            return ServiceResult.Failure("Checklist item was not found.");
        }

        dbContext.ChecklistItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private IQueryable<Trip> FindOwnedTrip(Guid ownerId, Guid tripId)
    {
        return dbContext.Trips.Where(trip => trip.OwnerId == ownerId && trip.Id == tripId);
    }

    private Task<bool> TripExistsAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken)
    {
        return FindOwnedTrip(ownerId, tripId).AnyAsync(cancellationToken);
    }

    private static string? ValidateTripDatesAndBudget(DateOnly startDate, DateOnly endDate, decimal plannedBudget)
    {
        if (endDate < startDate)
        {
            return "Trip end date cannot be before start date.";
        }

        if (plannedBudget < 0)
        {
            return "Planned budget cannot be negative.";
        }

        return null;
    }

    private static string? ValidateDestinationDates(DateOnly arrivalDate, DateOnly departureDate)
    {
        if (departureDate < arrivalDate)
        {
            return "Destination departure date cannot be before arrival date.";
        }

        return null;
    }

    private static string? ValidateActivity(decimal estimatedCost)
    {
        if (estimatedCost < 0)
        {
            return "Estimated cost cannot be negative.";
        }

        return null;
    }

    private static string? ValidateExpense(decimal amount)
    {
        if (amount <= 0)
        {
            return "Expense amount must be greater than zero.";
        }

        return null;
    }
}
