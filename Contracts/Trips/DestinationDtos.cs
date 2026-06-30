using System.ComponentModel.DataAnnotations;

namespace Contracts.Trips;

public sealed record DestinationDto(
    Guid Id,
    Guid TripId,
    string Name,
    string Location,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    string Description);

public sealed record CreateDestinationRequest(
    [Required, MaxLength(120)] string Name,
    [Required, MaxLength(200)] string Location,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    [MaxLength(1000)] string Description);

public sealed record UpdateDestinationRequest(
    [Required, MaxLength(120)] string Name,
    [Required, MaxLength(200)] string Location,
    DateOnly ArrivalDate,
    DateOnly DepartureDate,
    [MaxLength(1000)] string Description);
