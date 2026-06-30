using System.ComponentModel.DataAnnotations;
using Contracts.Common;

namespace Contracts.Trips;

public sealed record ActivityDto(
    Guid Id,
    Guid TripId,
    string Title,
    DateOnly Date,
    TimeOnly Time,
    string Location,
    string Description,
    decimal EstimatedCost,
    ActivityStatus Status);

public sealed record CreateActivityRequest(
    [Required, MaxLength(120)] string Title,
    DateOnly Date,
    TimeOnly Time,
    [MaxLength(200)] string Location,
    [MaxLength(1000)] string Description,
    [Range(0, double.MaxValue)] decimal EstimatedCost,
    ActivityStatus Status);

public sealed record UpdateActivityRequest(
    [Required, MaxLength(120)] string Title,
    DateOnly Date,
    TimeOnly Time,
    [MaxLength(200)] string Location,
    [MaxLength(1000)] string Description,
    [Range(0, double.MaxValue)] decimal EstimatedCost,
    ActivityStatus Status);
