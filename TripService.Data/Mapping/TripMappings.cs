using Contracts.Trips;
using TripService.Data.Models;
using ContractActivityStatus = Contracts.Common.ActivityStatus;
using ContractExpenseCategory = Contracts.Common.ExpenseCategory;

namespace TripService.Data.Mapping;

public static class TripMappings
{
    public static TripDto ToDto(this Trip trip)
    {
        var totalExpenses = trip.Expenses.Sum(expense => expense.Amount);

        return new TripDto(
            trip.Id,
            trip.OwnerId,
            trip.Title,
            trip.Description,
            trip.StartDate,
            trip.EndDate,
            trip.PlannedBudget,
            totalExpenses,
            trip.PlannedBudget - totalExpenses,
            trip.Notes,
            trip.CreatedAtUtc,
            trip.UpdatedAtUtc);
    }

    public static TripSummaryDto ToSummaryDto(this Trip trip)
    {
        var totalExpenses = trip.Expenses.Sum(expense => expense.Amount);

        return new TripSummaryDto(
            trip.Id,
            trip.Title,
            trip.StartDate,
            trip.EndDate,
            trip.PlannedBudget,
            totalExpenses,
            trip.PlannedBudget - totalExpenses);
    }

    public static DestinationDto ToDto(this Destination destination)
    {
        return new DestinationDto(
            destination.Id,
            destination.TripId,
            destination.Name,
            destination.Location,
            destination.ArrivalDate,
            destination.DepartureDate,
            destination.Description);
    }

    public static ActivityDto ToDto(this TripActivity activity)
    {
        return new ActivityDto(
            activity.Id,
            activity.TripId,
            activity.Title,
            activity.Date,
            activity.Time,
            activity.Location,
            activity.Description,
            activity.EstimatedCost,
            (ContractActivityStatus)activity.Status);
    }

    public static ExpenseDto ToDto(this Expense expense)
    {
        return new ExpenseDto(
            expense.Id,
            expense.TripId,
            expense.Name,
            (ContractExpenseCategory)expense.Category,
            expense.Amount,
            expense.Date,
            expense.Description);
    }

    public static ChecklistItemDto ToDto(this ChecklistItem item)
    {
        return new ChecklistItemDto(
            item.Id,
            item.TripId,
            item.Text,
            item.IsCompleted);
    }

    public static BudgetSummaryDto ToBudgetSummaryDto(this Trip trip)
    {
        var totalExpenses = trip.Expenses.Sum(expense => expense.Amount);

        return new BudgetSummaryDto(
            trip.Id,
            trip.PlannedBudget,
            totalExpenses,
            trip.PlannedBudget - totalExpenses);
    }

    public static Trip ToEntity(this CreateTripRequest request, Guid ownerId)
    {
        return new Trip
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PlannedBudget = request.PlannedBudget,
            Notes = request.Notes.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static Destination ToEntity(this CreateDestinationRequest request, Guid tripId)
    {
        return new Destination
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Name = request.Name.Trim(),
            Location = request.Location.Trim(),
            ArrivalDate = request.ArrivalDate,
            DepartureDate = request.DepartureDate,
            Description = request.Description.Trim()
        };
    }

    public static TripActivity ToEntity(this CreateActivityRequest request, Guid tripId)
    {
        return new TripActivity
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Title = request.Title.Trim(),
            Date = request.Date,
            Time = request.Time,
            Location = request.Location.Trim(),
            Description = request.Description.Trim(),
            EstimatedCost = request.EstimatedCost,
            Status = (ActivityStatus)request.Status
        };
    }

    public static Expense ToEntity(this CreateExpenseRequest request, Guid tripId)
    {
        return new Expense
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Name = request.Name.Trim(),
            Category = (ExpenseCategory)request.Category,
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description.Trim()
        };
    }

    public static ChecklistItem ToEntity(this CreateChecklistItemRequest request, Guid tripId)
    {
        return new ChecklistItem
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Text = request.Text.Trim(),
            IsCompleted = false
        };
    }

    public static void Apply(this UpdateTripRequest request, Trip trip)
    {
        trip.Title = request.Title.Trim();
        trip.Description = request.Description.Trim();
        trip.StartDate = request.StartDate;
        trip.EndDate = request.EndDate;
        trip.PlannedBudget = request.PlannedBudget;
        trip.Notes = request.Notes.Trim();
        trip.UpdatedAtUtc = DateTime.UtcNow;
    }

    public static void Apply(this UpdateDestinationRequest request, Destination destination)
    {
        destination.Name = request.Name.Trim();
        destination.Location = request.Location.Trim();
        destination.ArrivalDate = request.ArrivalDate;
        destination.DepartureDate = request.DepartureDate;
        destination.Description = request.Description.Trim();
    }

    public static void Apply(this UpdateActivityRequest request, TripActivity activity)
    {
        activity.Title = request.Title.Trim();
        activity.Date = request.Date;
        activity.Time = request.Time;
        activity.Location = request.Location.Trim();
        activity.Description = request.Description.Trim();
        activity.EstimatedCost = request.EstimatedCost;
        activity.Status = (ActivityStatus)request.Status;
    }

    public static void Apply(this UpdateExpenseRequest request, Expense expense)
    {
        expense.Name = request.Name.Trim();
        expense.Category = (ExpenseCategory)request.Category;
        expense.Amount = request.Amount;
        expense.Date = request.Date;
        expense.Description = request.Description.Trim();
    }

    public static void Apply(this UpdateChecklistItemRequest request, ChecklistItem item)
    {
        item.Text = request.Text.Trim();
        item.IsCompleted = request.IsCompleted;
    }
}
