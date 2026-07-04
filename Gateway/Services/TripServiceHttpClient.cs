using Contracts.Sharing;
using Contracts.Trips;
using System.Net.Http.Json;

namespace Gateway.Services;

public sealed class TripServiceHttpClient
{
    private readonly HttpClient httpClient;

    public TripServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(configuration["InternalServices:TripServiceBaseUrl"] ?? "http://localhost:8924");
    }

    public Task<HttpResponseMessage> GetTripsAsync(Guid ownerId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/users/{ownerId}/trips", cancellationToken);

    public Task<HttpResponseMessage> GetTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/users/{ownerId}/trips/{tripId}", cancellationToken);

    public Task<HttpResponseMessage> CreateTripAsync(Guid ownerId, CreateTripRequest request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync($"/internal/users/{ownerId}/trips", request, cancellationToken);

    public Task<HttpResponseMessage> UpdateTripAsync(Guid ownerId, Guid tripId, UpdateTripRequest request, CancellationToken cancellationToken) =>
        httpClient.PutAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}", request, cancellationToken);

    public Task<HttpResponseMessage> DeleteTripAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken) =>
        httpClient.DeleteAsync($"/internal/users/{ownerId}/trips/{tripId}", cancellationToken);

    public Task<HttpResponseMessage> GetDestinationsAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/users/{ownerId}/trips/{tripId}/destinations", cancellationToken);

    public Task<HttpResponseMessage> CreateDestinationAsync(Guid ownerId, Guid tripId, CreateDestinationRequest request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/destinations", request, cancellationToken);

    public Task<HttpResponseMessage> UpdateDestinationAsync(Guid ownerId, Guid tripId, Guid destinationId, UpdateDestinationRequest request, CancellationToken cancellationToken) =>
        httpClient.PutAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/destinations/{destinationId}", request, cancellationToken);

    public Task<HttpResponseMessage> DeleteDestinationAsync(Guid ownerId, Guid tripId, Guid destinationId, CancellationToken cancellationToken) =>
        httpClient.DeleteAsync($"/internal/users/{ownerId}/trips/{tripId}/destinations/{destinationId}", cancellationToken);

    public Task<HttpResponseMessage> GetActivitiesAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/users/{ownerId}/trips/{tripId}/activities", cancellationToken);

    public Task<HttpResponseMessage> CreateActivityAsync(Guid ownerId, Guid tripId, CreateActivityRequest request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/activities", request, cancellationToken);

    public Task<HttpResponseMessage> UpdateActivityAsync(Guid ownerId, Guid tripId, Guid activityId, UpdateActivityRequest request, CancellationToken cancellationToken) =>
        httpClient.PutAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/activities/{activityId}", request, cancellationToken);

    public Task<HttpResponseMessage> DeleteActivityAsync(Guid ownerId, Guid tripId, Guid activityId, CancellationToken cancellationToken) =>
        httpClient.DeleteAsync($"/internal/users/{ownerId}/trips/{tripId}/activities/{activityId}", cancellationToken);

    public Task<HttpResponseMessage> GetExpensesAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/users/{ownerId}/trips/{tripId}/expenses", cancellationToken);

    public Task<HttpResponseMessage> CreateExpenseAsync(Guid ownerId, Guid tripId, CreateExpenseRequest request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/expenses", request, cancellationToken);

    public Task<HttpResponseMessage> UpdateExpenseAsync(Guid ownerId, Guid tripId, Guid expenseId, UpdateExpenseRequest request, CancellationToken cancellationToken) =>
        httpClient.PutAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/expenses/{expenseId}", request, cancellationToken);

    public Task<HttpResponseMessage> DeleteExpenseAsync(Guid ownerId, Guid tripId, Guid expenseId, CancellationToken cancellationToken) =>
        httpClient.DeleteAsync($"/internal/users/{ownerId}/trips/{tripId}/expenses/{expenseId}", cancellationToken);

    public Task<HttpResponseMessage> GetBudgetAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/users/{ownerId}/trips/{tripId}/budget", cancellationToken);

    public Task<HttpResponseMessage> GetChecklistItemsAsync(Guid ownerId, Guid tripId, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/users/{ownerId}/trips/{tripId}/checklist-items", cancellationToken);

    public Task<HttpResponseMessage> CreateChecklistItemAsync(Guid ownerId, Guid tripId, CreateChecklistItemRequest request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/checklist-items", request, cancellationToken);

    public Task<HttpResponseMessage> UpdateChecklistItemAsync(Guid ownerId, Guid tripId, Guid itemId, UpdateChecklistItemRequest request, CancellationToken cancellationToken) =>
        httpClient.PutAsJsonAsync($"/internal/users/{ownerId}/trips/{tripId}/checklist-items/{itemId}", request, cancellationToken);

    public Task<HttpResponseMessage> DeleteChecklistItemAsync(Guid ownerId, Guid tripId, Guid itemId, CancellationToken cancellationToken) =>
        httpClient.DeleteAsync($"/internal/users/{ownerId}/trips/{tripId}/checklist-items/{itemId}", cancellationToken);

    public Task<HttpResponseMessage> CreateShareLinkAsync(Guid ownerId, CreateShareLinkRequest request, CancellationToken cancellationToken) =>
        httpClient.PostAsJsonAsync($"/internal/users/{ownerId}/share-links", request, cancellationToken);

    public Task<HttpResponseMessage> GetSharedTripAsync(string token, CancellationToken cancellationToken) =>
        httpClient.GetAsync($"/internal/shared-trips/{Uri.EscapeDataString(token)}", cancellationToken);

    public Task<HttpResponseMessage> UpdateSharedTripAsync(string token, UpdateTripRequest request, CancellationToken cancellationToken) =>
        httpClient.PutAsJsonAsync($"/internal/shared-trips/{Uri.EscapeDataString(token)}/trip", request, cancellationToken);
}
