namespace TripService.Data.Models;

public sealed class Expense
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public Trip? Trip { get; set; }

    public string Name { get; set; } = string.Empty;

    public ExpenseCategory Category { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public string Description { get; set; } = string.Empty;
}
