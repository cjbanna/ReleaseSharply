using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace ReleaseSharply.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReleaseSharply(this IServiceCollection services, Action<ReleaseSharplyBuilderOptions> configure)
        {
            var options = new ReleaseSharplyBuilderOptions();
            configure(options);

            if (options.Clients?.Any() == false)
            {
                throw new ArgumentException("Clients cannot be empty", nameof(options.Clients));
            }

            if (options.ApiResources?.Any() == false)
            {
                throw new ArgumentException("ApiResources cannot be empty", nameof(options.ApiResources));
            }

            if (options.ApiScopes?.Any() == false)
            {
                throw new ArgumentException("ApiScopes cannot be empty", nameof(options.ApiScopes));
            }

            var isDevelopment = false;
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var env = scope.ServiceProvider.GetService<IWebHostEnvironment>();
                isDevelopment = env.IsDevelopment();
            }

            var builder = services.AddIdentityServer()
                .AddInMemoryClients(options.Clients)
                .AddInMemoryApiResources(options.ApiResources)
                .AddInMemoryApiScopes(options.ApiScopes);

            if (isDevelopment)
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                if (options.SigningCredentials == null)
                {
                    throw new ArgumentNullException("signingCredentials are required for non-Development environments", nameof(options.SigningCredentials));
                }

                builder.AddSigningCredential(options.SigningCredentials);
            }

            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy(nameof(Policies.FeatureFlagRead), policy =>
                    {
                        policy
                            .RequireAuthenticatedUser()
                            .RequireClaim("scope", Scopes.Read);

                    });

                    options.AddPolicy(nameof(Policies.FeatureFlagWrite), policy =>
                    {
                        policy
                            .RequireAuthenticatedUser()
                            .RequireClaim("scope", Scopes.Write);
                    });
                })
                .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:5001";
                });


            services.AddSignalR();
            return services.AddSingleton<FeatureHub>();
        }
    }
}
