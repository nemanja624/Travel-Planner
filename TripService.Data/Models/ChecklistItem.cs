namespace TripService.Data.Models;

public sealed class ChecklistItem
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public Trip? Trip { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }
}
