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

    private IQueryable<Trip> FindOwnedTrip(Guid ownerId, Guid tripId)
    {
        return dbContext.Trips.Where(trip => trip.OwnerId == ownerId && trip.Id == tripId);
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
}
