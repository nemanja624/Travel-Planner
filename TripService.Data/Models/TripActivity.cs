namespace TripService.Data.Models;

public sealed class TripActivity
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public Trip? Trip { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    public string Location { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal EstimatedCost { get; set; }

    public ActivityStatus Status { get; set; }
}
