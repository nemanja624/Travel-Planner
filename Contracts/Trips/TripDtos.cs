using System.ComponentModel.DataAnnotations;

namespace Contracts.Trips;

public sealed record TripDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal PlannedBudget,
    decimal TotalExpenses,
    decimal RemainingBudget,
    string Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record TripSummaryDto(
    Guid Id,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal PlannedBudget,
    decimal TotalExpenses,
    decimal RemainingBudget);

public sealed record AdminTripSummaryDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal PlannedBudget,
    decimal TotalExpenses,
    decimal RemainingBudget,
    DateTime CreatedAtUtc);

public sealed record CreateTripRequest(
    [Required, MaxLength(120)] string Title,
    [MaxLength(1000)] string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    [Range(0, double.MaxValue)] decimal PlannedBudget,
    [MaxLength(2000)] string Notes);

public sealed record UpdateTripRequest(
    [Required, MaxLength(120)] string Title,
    [MaxLength(1000)] string Description,
    DateOnly StartDate,
    DateOnly EndDate,
    [Range(0, double.MaxValue)] decimal PlannedBudget,
    [MaxLength(2000)] string Notes);
