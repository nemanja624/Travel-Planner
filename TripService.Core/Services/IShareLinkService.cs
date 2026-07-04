using Contracts.Sharing;
using Contracts.Trips;
using TripService.Core.Common;

namespace TripService.Core.Services;

public interface IShareLinkService
{
    Task<ServiceResult<ShareLinkDto>> CreateShareLinkAsync(Guid ownerId, CreateShareLinkRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<SharedTripDto>> GetSharedTripAsync(string token, CancellationToken cancellationToken = default);

    Task<ServiceResult<TripDto>> UpdateSharedTripAsync(string token, UpdateTripRequest request, CancellationToken cancellationToken = default);
}
