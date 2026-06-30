namespace TripService.Data.Models;

public sealed class Trip
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal PlannedBudget { get; set; }

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Destination> Destinations { get; set; } = new List<Destination>();

    public ICollection<TripActivity> Activities { get; set; } = new List<TripActivity>();

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();
}
