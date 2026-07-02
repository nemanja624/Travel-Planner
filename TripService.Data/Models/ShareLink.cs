using Contracts.Common;

namespace TripService.Data.Models;

public sealed class ShareLink
{
    public Guid Id { get; set; }

    public Guid TripId { get; set; }

    public Trip? Trip { get; set; }

    public ShareAccessLevel AccessLevel { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }
}
