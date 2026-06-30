using System.ComponentModel.DataAnnotations;

namespace Contracts.Trips;

public sealed record ChecklistItemDto(
    Guid Id,
    Guid TripId,
    string Text,
    bool IsCompleted);

public sealed record CreateChecklistItemRequest(
    [Required, MaxLength(200)] string Text);

public sealed record UpdateChecklistItemRequest(
    [Required, MaxLength(200)] string Text,
    bool IsCompleted);
