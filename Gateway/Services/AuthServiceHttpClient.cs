using Contracts.Auth;
using Contracts.Common;
using System.Net.Http.Json;

namespace Gateway.Services;

public sealed class AuthServiceHttpClient
{
    private readonly HttpClient httpClient;

    public AuthServiceHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(configuration["InternalServices:AuthServiceBaseUrl"] ?? "http://localhost:8923");
    }

    public Task<HttpResponseMessage> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        return httpClient.PostAsJsonAsync("/internal/auth/register", request, cancellationToken);
    }

    public Task<HttpResponseMessage> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        return httpClient.PostAsJsonAsync("/internal/auth/login", request, cancellationToken);
    }

    public Task<HttpResponseMessage> GetUsersAsync(CancellationToken cancellationToken)
    {
        return httpClient.GetAsync("/internal/admin/users", cancellationToken);
    }

    public Task<HttpResponseMessage> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        return httpClient.PutAsJsonAsync($"/internal/admin/users/{userId}/role", request, cancellationToken);
    }

    public Task<HttpResponseMessage> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        return httpClient.PutAsJsonAsync($"/internal/admin/users/{userId}/status", request, cancellationToken);
    }
}
