using System.ComponentModel.DataAnnotations;
using Contracts.Common;

namespace Contracts.Trips;

public sealed record ExpenseDto(
    Guid Id,
    Guid TripId,
    string Name,
    ExpenseCategory Category,
    decimal Amount,
    DateOnly Date,
    string Description);

public sealed record CreateExpenseRequest(
    [Required, MaxLength(120)] string Name,
    ExpenseCategory Category,
    [Range(0.01, double.MaxValue)] decimal Amount,
    DateOnly Date,
    [MaxLength(1000)] string Description);

public sealed record UpdateExpenseRequest(
    [Required, MaxLength(120)] string Name,
    ExpenseCategory Category,
    [Range(0.01, double.MaxValue)] decimal Amount,
    DateOnly Date,
    [MaxLength(1000)] string Description);

public sealed record BudgetSummaryDto(
    Guid TripId,
    decimal PlannedBudget,
    decimal TotalExpenses,
    decimal RemainingBudget);
