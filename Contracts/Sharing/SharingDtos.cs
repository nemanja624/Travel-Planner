using System.ComponentModel.DataAnnotations;
using Contracts.Common;
using Contracts.Trips;

namespace Contracts.Sharing;

public sealed record CreateShareLinkRequest(
    Guid TripId,
    ShareAccessLevel AccessLevel,
    DateTime ExpiresAtUtc);

public sealed record ShareLinkDto(
    Guid Id,
    Guid TripId,
    ShareAccessLevel AccessLevel,
    string Token,
    string ShareUrl,
    string QrCodeUrl,
    DateTime ExpiresAtUtc,
    DateTime CreatedAtUtc);

public sealed record SharedTripDto(
    TripDto Trip,
    IReadOnlyCollection<DestinationDto> Destinations,
    IReadOnlyCollection<ActivityDto> Activities,
    IReadOnlyCollection<ExpenseDto> Expenses,
    IReadOnlyCollection<ChecklistItemDto> ChecklistItems,
    ShareAccessLevel AccessLevel);

public sealed record ValidateShareTokenRequest(
    [Required] string Token);
