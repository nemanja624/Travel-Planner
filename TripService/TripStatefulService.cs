using Contracts.Sharing;
using Contracts.Trips;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;
using TripService.Core.Common;
using TripService.Core.Services;
using TripService.Data;

namespace TripService;

internal sealed class TripStatefulService : StatefulService
{
    public TripStatefulService(StatefulServiceContext context)
        : base(context)
    {
    }

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return
        [
            new ServiceReplicaListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, "TripServiceEndpoint", (url, listener) =>
                {
                    var builder = WebApplication.CreateBuilder();
                    builder.Services.AddSingleton<StatefulServiceContext>(serviceContext);
                    builder.WebHost
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                        .UseUrls(url);

                    builder.Services.AddDbContext<TripDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("TravelPlannerDatabase")));
                    builder.Services.AddScoped<ITripPlannerService, TripPlannerService>();
                    builder.Services.AddSingleton(builder.Configuration.GetSection("ShareLinks").Get<ShareLinkOptions>() ?? new ShareLinkOptions());
                    builder.Services.AddScoped<IShareLinkService, ShareLinkService>();

                    var app = builder.Build();

                    app.MapGet("/internal/health", () => Results.Ok(new { service = "trip", status = "ok" }));

                    app.MapGet("/internal/users/{ownerId:guid}/trips", async (Guid ownerId, ITripPlannerService service, CancellationToken ct) =>
                        Results.Ok(await service.GetTripsAsync(ownerId, ct)));
                    app.MapGet("/internal/admin/trips", async (ITripPlannerService service, CancellationToken ct) =>
                        Results.Ok(await service.GetAllTripsAsync(ct)));
                    app.MapDelete("/internal/admin/trips/{tripId:guid}", async (Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.DeleteAnyTripAsync(tripId, ct)));
                    app.MapGet("/internal/users/{ownerId:guid}/trips/{tripId:guid}", async (Guid ownerId, Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.GetTripAsync(ownerId, tripId, ct)));
                    app.MapPost("/internal/users/{ownerId:guid}/trips", async (Guid ownerId, CreateTripRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.CreateTripAsync(ownerId, request, ct)));
                    app.MapPut("/internal/users/{ownerId:guid}/trips/{tripId:guid}", async (Guid ownerId, Guid tripId, UpdateTripRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.UpdateTripAsync(ownerId, tripId, request, ct)));
                    app.MapDelete("/internal/users/{ownerId:guid}/trips/{tripId:guid}", async (Guid ownerId, Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.DeleteTripAsync(ownerId, tripId, ct)));

                    app.MapGet("/internal/users/{ownerId:guid}/trips/{tripId:guid}/destinations", async (Guid ownerId, Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.GetDestinationsAsync(ownerId, tripId, ct)));
                    app.MapPost("/internal/users/{ownerId:guid}/trips/{tripId:guid}/destinations", async (Guid ownerId, Guid tripId, CreateDestinationRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.CreateDestinationAsync(ownerId, tripId, request, ct)));
                    app.MapPut("/internal/users/{ownerId:guid}/trips/{tripId:guid}/destinations/{destinationId:guid}", async (Guid ownerId, Guid tripId, Guid destinationId, UpdateDestinationRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.UpdateDestinationAsync(ownerId, tripId, destinationId, request, ct)));
                    app.MapDelete("/internal/users/{ownerId:guid}/trips/{tripId:guid}/destinations/{destinationId:guid}", async (Guid ownerId, Guid tripId, Guid destinationId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.DeleteDestinationAsync(ownerId, tripId, destinationId, ct)));

                    app.MapGet("/internal/users/{ownerId:guid}/trips/{tripId:guid}/activities", async (Guid ownerId, Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.GetActivitiesAsync(ownerId, tripId, ct)));
                    app.MapPost("/internal/users/{ownerId:guid}/trips/{tripId:guid}/activities", async (Guid ownerId, Guid tripId, CreateActivityRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.CreateActivityAsync(ownerId, tripId, request, ct)));
                    app.MapPut("/internal/users/{ownerId:guid}/trips/{tripId:guid}/activities/{activityId:guid}", async (Guid ownerId, Guid tripId, Guid activityId, UpdateActivityRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.UpdateActivityAsync(ownerId, tripId, activityId, request, ct)));
                    app.MapDelete("/internal/users/{ownerId:guid}/trips/{tripId:guid}/activities/{activityId:guid}", async (Guid ownerId, Guid tripId, Guid activityId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.DeleteActivityAsync(ownerId, tripId, activityId, ct)));

                    app.MapGet("/internal/users/{ownerId:guid}/trips/{tripId:guid}/expenses", async (Guid ownerId, Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.GetExpensesAsync(ownerId, tripId, ct)));
                    app.MapPost("/internal/users/{ownerId:guid}/trips/{tripId:guid}/expenses", async (Guid ownerId, Guid tripId, CreateExpenseRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.CreateExpenseAsync(ownerId, tripId, request, ct)));
                    app.MapPut("/internal/users/{ownerId:guid}/trips/{tripId:guid}/expenses/{expenseId:guid}", async (Guid ownerId, Guid tripId, Guid expenseId, UpdateExpenseRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.UpdateExpenseAsync(ownerId, tripId, expenseId, request, ct)));
                    app.MapDelete("/internal/users/{ownerId:guid}/trips/{tripId:guid}/expenses/{expenseId:guid}", async (Guid ownerId, Guid tripId, Guid expenseId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.DeleteExpenseAsync(ownerId, tripId, expenseId, ct)));

                    app.MapGet("/internal/users/{ownerId:guid}/trips/{tripId:guid}/budget", async (Guid ownerId, Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.GetBudgetSummaryAsync(ownerId, tripId, ct)));
                    app.MapGet("/internal/users/{ownerId:guid}/trips/{tripId:guid}/checklist-items", async (Guid ownerId, Guid tripId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.GetChecklistItemsAsync(ownerId, tripId, ct)));
                    app.MapPost("/internal/users/{ownerId:guid}/trips/{tripId:guid}/checklist-items", async (Guid ownerId, Guid tripId, CreateChecklistItemRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.CreateChecklistItemAsync(ownerId, tripId, request, ct)));
                    app.MapPut("/internal/users/{ownerId:guid}/trips/{tripId:guid}/checklist-items/{itemId:guid}", async (Guid ownerId, Guid tripId, Guid itemId, UpdateChecklistItemRequest request, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.UpdateChecklistItemAsync(ownerId, tripId, itemId, request, ct)));
                    app.MapDelete("/internal/users/{ownerId:guid}/trips/{tripId:guid}/checklist-items/{itemId:guid}", async (Guid ownerId, Guid tripId, Guid itemId, ITripPlannerService service, CancellationToken ct) =>
                        ToResult(await service.DeleteChecklistItemAsync(ownerId, tripId, itemId, ct)));

                    app.MapPost("/internal/users/{ownerId:guid}/share-links", async (Guid ownerId, CreateShareLinkRequest request, IShareLinkService service, CancellationToken ct) =>
                        ToResult(await service.CreateShareLinkAsync(ownerId, request, ct)));
                    app.MapGet("/internal/shared-trips/{token}", async (string token, IShareLinkService service, CancellationToken ct) =>
                        ToResult(await service.GetSharedTripAsync(token, ct)));
                    app.MapPut("/internal/shared-trips/{token}/trip", async (string token, UpdateTripRequest request, IShareLinkService service, CancellationToken ct) =>
                        ToResult(await service.UpdateSharedTripAsync(token, request, ct), isForbidden: error => error?.Contains("does not allow editing", StringComparison.OrdinalIgnoreCase) == true));

                    return app;
                }))
        ];
    }

    private static IResult ToResult(ServiceResult result)
    {
        if (result.Succeeded)
        {
            return Results.NoContent();
        }

        return IsNotFound(result.Error)
            ? Results.NotFound(new { error = result.Error })
            : Results.BadRequest(new { error = result.Error });
    }

    private static IResult ToResult<T>(ServiceResult<T> result, Func<string?, bool>? isForbidden = null)
    {
        if (result.Succeeded && result.Value is not null)
        {
            return Results.Ok(result.Value);
        }

        if (isForbidden?.Invoke(result.Error) == true)
        {
            return Results.Json(new { error = result.Error }, statusCode: StatusCodes.Status403Forbidden);
        }

        return IsNotFound(result.Error)
            ? Results.NotFound(new { error = result.Error })
            : Results.BadRequest(new { error = result.Error });
    }

    private static bool IsNotFound(string? error)
    {
        return error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true;
    }
}
