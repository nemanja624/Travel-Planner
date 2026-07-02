using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Core.Security;
using AuthService.Core.Services;
using AuthService.Data;
using Gateway.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using TripService.Core.Services;
using TripService.Data;

namespace Gateway
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class Gateway : StatelessService
    {
        public Gateway(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);
                        builder.Services.AddControllers();
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();
                        builder.Services.AddDbContext<TripDbContext>(options =>
                            options.UseSqlServer(builder.Configuration.GetConnectionString("TravelPlannerDatabase")));
                        builder.Services.AddDbContext<AuthDbContext>(options =>
                            options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDatabase")));
                        builder.Services.AddHttpContextAccessor();
                        builder.Services.AddScoped<ITripPlannerService, TripPlannerService>();
                        builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
                        builder.Services.AddSingleton(builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions());
                        builder.Services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
                        builder.Services.AddScoped<IAccessTokenValidator, JwtAccessTokenValidator>();
                        builder.Services.AddScoped<IAuthService, AuthService.Core.Services.AuthService>();
                        builder.Services.AddScoped<IAdminUserService, AdminUserService>();
                        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                        builder.Services.AddScoped<IRoleGuardService, RoleGuardService>();
                        var app = builder.Build();
                        if (app.Environment.IsDevelopment())
                        {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                        }
                        app.UseAuthorization();
                        app.MapControllers();
                        
                        return app;

                    }))
            };
        }
    }
}
