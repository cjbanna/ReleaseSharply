using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReleaseSharply.Server.Data;
using ReleaseSharply.Server.Options;
using System;
using System.Reflection;

namespace ReleaseSharply.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReleaseSharply(this IServiceCollection services, Action<ReleaseSharplyOptions> configure)
        {
            var options = new ReleaseSharplyOptions();
            configure(options);

            services.AddSignalR();

            services
                .AddSingleton<FeatureHub>()
                .AddTransient<IFeatureManager, FeatureManager>();

            services.AddDbContext<FeaturesDbContext>(dbContextOptions =>
            {
                if (!string.IsNullOrWhiteSpace(options.SqlServerConnectionString))
                {
                    dbContextOptions.UseSqlServer(options.SqlServerConnectionString);
                }
                else
                {
                    dbContextOptions.UseSqlite("Filename=ReleaseSharply.db");
                }
            });

            var configOptions = new ReleaseSharplyOptions();

            var isDevelopment = false;
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var env = scope.ServiceProvider.GetService<IWebHostEnvironment>();
                isDevelopment = env.IsDevelopment();

                //var dbContext = scope.ServiceProvider.GetService<FeaturesDbContext>();
                //dbContext.Database.EnsureCreated();
            }

            var builder = services.AddIdentityServer();

            if (string.IsNullOrWhiteSpace(options.SqlServerConnectionString))
            {
                var apiResources = new[]
                {
                    new ApiResource
                    {
                        Name = "Features",
                        Scopes = new [] { Scopes.Read, Scopes.Write }
                    }
                };

                var apiScopes = new[]
                {
                    new ApiScope(Scopes.Read, ""),
                    new ApiScope(Scopes.Write, "")
                };

                builder
                    .AddInMemoryClients(options.Clients)
                    .AddInMemoryApiResources(apiResources)
                    .AddInMemoryApiScopes(apiScopes);
            }
            else
            {
                var migrationsAssembly = typeof(ServiceCollectionExtensions).GetTypeInfo().Assembly.GetName().Name;
                builder
                    .AddConfigurationStore(storeOptions =>
                    {
                        storeOptions.ConfigureDbContext = b => b.UseSqlServer(options.SqlServerConnectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    })
                    .AddOperationalStore(storeOptions =>
                    {
                        storeOptions.ConfigureDbContext = b => b.UseSqlServer(options.SqlServerConnectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    });
            }

            if (options.SigningCredentials != null)
            {
                builder.AddSigningCredential(options.SigningCredentials);
            }
            else if (isDevelopment)
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new ArgumentNullException("signingCredentials are required for non-Development environments", nameof(options.SigningCredentials));
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
                    options.Authority = isDevelopment ? options.Authority ?? "https://localhost:5001" : options.Authority;
                });

            return services;
        }
    }
}
