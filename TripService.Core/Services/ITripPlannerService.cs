using Contracts.Trips;
using TripService.Core.Common;

namespace TripService.Core.Services;

public interface ITripPlannerService
{
    Task<IReadOnlyCollection<TripSummaryDto>> GetTripsAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<ServiceResult<TripDto>> GetTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);

    Task<ServiceResult<TripDto>> CreateTripAsync(Guid ownerId, CreateTripRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<TripDto>> UpdateTripAsync(Guid ownerId, Guid tripId, UpdateTripRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken = default);
}
