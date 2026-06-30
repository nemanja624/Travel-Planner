namespace TripService.Data.Models;

public sealed class Destination
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public Trip? Trip { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateOnly ArrivalDate { get; set; }

    public DateOnly DepartureDate { get; set; }

    public string Description { get; set; } = string.Empty;
}
