using AuthService.Core.Common;
using AuthService.Core.Security;
using AuthService.Core.Services;
using AuthService.Data;
using Contracts.Auth;
using Contracts.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace Gateway;

internal sealed class AuthStatefulService : StatefulService
{
    public AuthStatefulService(StatefulServiceContext context)
        : base(context)
    {
    }

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return
        [
            new ServiceReplicaListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, "AuthServiceEndpoint", (url, listener) =>
                {
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Auth service on {url}");

                    var builder = WebApplication.CreateBuilder();
                    builder.Services.AddSingleton<StatefulServiceContext>(serviceContext);
                    builder.WebHost
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                        .UseUrls(url);

                    builder.Services.AddDbContext<AuthDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDatabase")));
                    builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
                    builder.Services.AddSingleton(builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions());
                    builder.Services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
                    builder.Services.AddScoped<IAuthService, AuthService.Core.Services.AuthService>();
                    builder.Services.AddScoped<IAdminUserService, AdminUserService>();

                    var app = builder.Build();

                    app.MapGet("/internal/health", () => Results.Ok(new { service = "auth", status = "ok" }));
                    app.MapPost("/internal/auth/register", async (
                        RegisterUserRequest request,
                        IAuthService authService,
                        CancellationToken cancellationToken) =>
                        ToAuthResult(await authService.RegisterAsync(request, cancellationToken)));
                    app.MapPost("/internal/auth/login", async (
                        LoginRequest request,
                        IAuthService authService,
                        CancellationToken cancellationToken) =>
                        ToAuthResult(await authService.LoginAsync(request, cancellationToken)));
                    app.MapGet("/internal/admin/users", async (
                        IAdminUserService adminUserService,
                        CancellationToken cancellationToken) =>
                        Results.Ok(await adminUserService.GetUsersAsync(cancellationToken)));
                    app.MapPut("/internal/admin/users/{userId:guid}/role", async (
                        Guid userId,
                        UpdateUserRoleRequest request,
                        IAdminUserService adminUserService,
                        CancellationToken cancellationToken) =>
                        ToUserResult(await adminUserService.UpdateUserRoleAsync(userId, request.Role, cancellationToken)));
                    app.MapPut("/internal/admin/users/{userId:guid}/status", async (
                        Guid userId,
                        UpdateUserStatusRequest request,
                        IAdminUserService adminUserService,
                        CancellationToken cancellationToken) =>
                        ToUserResult(await adminUserService.UpdateUserStatusAsync(userId, request.IsActive, cancellationToken)));

                    return app;
                }))
        ];
    }

    private static IResult ToAuthResult(ServiceResult<AuthResponse> result)
    {
        return result.Succeeded && result.Value is not null
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error });
    }

    private static IResult ToUserResult(ServiceResult<UserDto> result)
    {
        return result.Succeeded && result.Value is not null
            ? Results.Ok(result.Value)
            : Results.NotFound(new { error = result.Error });
    }
}
