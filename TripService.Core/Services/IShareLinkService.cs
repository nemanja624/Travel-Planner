using Contracts.Sharing;
using TripService.Core.Common;

namespace TripService.Core.Services;

public interface IShareLinkService
{
    Task<ServiceResult<ShareLinkDto>> CreateShareLinkAsync(Guid ownerId, CreateShareLinkRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<SharedTripDto>> GetSharedTripAsync(string token, CancellationToken cancellationToken = default);
}
