using System.Security.Cryptography;
using Contracts.Common;
using Contracts.Sharing;
using Contracts.Trips;
using Microsoft.EntityFrameworkCore;
using TripService.Core.Common;
using TripService.Data;
using TripService.Data.Mapping;
using TripService.Data.Models;

namespace TripService.Core.Services;

public sealed class ShareLinkService : IShareLinkService
{
    private const int TokenBytes = 32;
    private readonly TripDbContext dbContext;
    private readonly ShareLinkOptions options;

    public ShareLinkService(TripDbContext dbContext, ShareLinkOptions options)
    {
        this.dbContext = dbContext;
        this.options = options;
    }

    public async Task<ServiceResult<ShareLinkDto>> CreateShareLinkAsync(Guid ownerId, CreateShareLinkRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return ServiceResult<ShareLinkDto>.Failure("Share link expiration must be in the future.");
        }

        var tripExists = await dbContext.Trips
            .AnyAsync(trip => trip.Id == request.TripId && trip.OwnerId == ownerId, cancellationToken);

        if (!tripExists)
        {
            return ServiceResult<ShareLinkDto>.Failure("Trip was not found.");
        }

        var token = GenerateToken();
        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            TripId = request.TripId,
            AccessLevel = request.AccessLevel,
            TokenHash = HashToken(token),
            ExpiresAtUtc = request.ExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

        await dbContext.ShareLinks.AddAsync(shareLink, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<ShareLinkDto>.Success(ToDto(shareLink, token));
    }

    public async Task<ServiceResult<SharedTripDto>> GetSharedTripAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ServiceResult<SharedTripDto>.Failure("Share token is required.");
        }

        var tokenHash = HashToken(token.Trim());
        var shareLink = await dbContext.ShareLinks
            .AsNoTracking()
            .Include(link => link.Trip)
                .ThenInclude(trip => trip!.Destinations)
            .Include(link => link.Trip)
                .ThenInclude(trip => trip!.Activities)
            .Include(link => link.Trip)
                .ThenInclude(trip => trip!.Expenses)
            .Include(link => link.Trip)
                .ThenInclude(trip => trip!.ChecklistItems)
            .FirstOrDefaultAsync(link => link.TokenHash == tokenHash, cancellationToken);

        if (shareLink is null || shareLink.Trip is null || shareLink.RevokedAtUtc is not null)
        {
            return ServiceResult<SharedTripDto>.Failure("Share token is invalid.");
        }

        if (shareLink.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return ServiceResult<SharedTripDto>.Failure("Share token has expired.");
        }

        return ServiceResult<SharedTripDto>.Success(new SharedTripDto(
            shareLink.Trip.ToDto(),
            shareLink.Trip.Destinations.OrderBy(destination => destination.ArrivalDate).Select(destination => destination.ToDto()).ToList(),
            shareLink.Trip.Activities.OrderBy(activity => activity.Date).ThenBy(activity => activity.Time).Select(activity => activity.ToDto()).ToList(),
            shareLink.Trip.Expenses.OrderBy(expense => expense.Date).Select(expense => expense.ToDto()).ToList(),
            shareLink.Trip.ChecklistItems.OrderBy(item => item.IsCompleted).ThenBy(item => item.Text).Select(item => item.ToDto()).ToList(),
            shareLink.AccessLevel));
    }

    public async Task<ServiceResult<TripDto>> UpdateSharedTripAsync(string token, UpdateTripRequest request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateTripDatesAndBudget(request.StartDate, request.EndDate, request.PlannedBudget);
        if (validationError is not null)
        {
            return ServiceResult<TripDto>.Failure(validationError);
        }

        var shareLinkResult = await FindValidShareLinkAsync(token, cancellationToken);
        if (!shareLinkResult.Succeeded || shareLinkResult.Value is null)
        {
            return ServiceResult<TripDto>.Failure(shareLinkResult.Error ?? "Share token is invalid.");
        }

        var shareLink = shareLinkResult.Value;
        if (shareLink.AccessLevel != ShareAccessLevel.Edit)
        {
            return ServiceResult<TripDto>.Failure("Share token does not allow editing.");
        }

        request.Apply(shareLink.Trip!); // azuriranje trip-a na osnovu request-a, menja trip sa novim vrednostima
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<TripDto>.Success(shareLink.Trip!.ToDto()); // konvertuj trip u DTO i vrati ga kao rezultat
    }

    private ShareLinkDto ToDto(ShareLink shareLink, string token)
    {
        var shareUrl = BuildShareUrl(token);

        return new ShareLinkDto(
            shareLink.Id,
            shareLink.TripId,
            shareLink.AccessLevel,
            token,
            shareUrl,
            BuildQrCodeUrl(shareUrl),
            shareLink.ExpiresAtUtc,
            shareLink.CreatedAtUtc);
    }

    private string BuildShareUrl(string token)
    {
        return $"{options.PublicBaseUrl.TrimEnd('/')}/shared/{Uri.EscapeDataString(token)}";
    }

    private string BuildQrCodeUrl(string shareUrl)
    {
        return options.QrCodeUrlTemplate.Replace("{shareUrl}", Uri.EscapeDataString(shareUrl));
    }

    private static string GenerateToken()
    {
        return Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
    }

    private async Task<ServiceResult<ShareLink>> FindValidShareLinkAsync(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ServiceResult<ShareLink>.Failure("Share token is required.");
        }

        var tokenHash = HashToken(token.Trim());
        var shareLink = await dbContext.ShareLinks
            .Include(link => link.Trip)
                .ThenInclude(trip => trip!.Expenses)
            .FirstOrDefaultAsync(link => link.TokenHash == tokenHash, cancellationToken);

        if (shareLink is null || shareLink.Trip is null || shareLink.RevokedAtUtc is not null)
        {
            return ServiceResult<ShareLink>.Failure("Share token is invalid.");
        }

        if (shareLink.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return ServiceResult<ShareLink>.Failure("Share token has expired.");
        }

        return ServiceResult<ShareLink>.Success(shareLink);
    }

    private static string? ValidateTripDatesAndBudget(DateOnly startDate, DateOnly endDate, decimal plannedBudget)
    {
        if (endDate < startDate)
        {
            return "End date cannot be before start date.";
        }

        if (plannedBudget < 0)
        {
            return "Planned budget cannot be negative.";
        }

        return null;
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
