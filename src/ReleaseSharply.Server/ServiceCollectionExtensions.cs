using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ReleaseSharply.Server.Data;
using ReleaseSharply.Server.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ReleaseSharply.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReleaseSharply(this IServiceCollection services, Action<ReleaseSharplyBuilderOptions> configure)
        {
            var options = new ReleaseSharplyBuilderOptions();
            configure(options);

            services.AddSignalR();

            services
                .AddSingleton<FeatureHub>()
                .AddTransient<FeaturesDbSeedDataGenerator>()
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

                var s = scope.ServiceProvider.GetService<FeaturesDbSeedDataGenerator>();
                s.SeedData();

                var configuration = scope.ServiceProvider.GetService<IConfiguration>();
                configuration.Bind(nameof(ReleaseSharplyOptions), configOptions);
            }

            var builder = services.AddIdentityServer();

            if (string.IsNullOrWhiteSpace(options.SqlServerConnectionString))
            {
                var apiResources = new[]
                {
                    new ApiResource
                    {
                        Name = "Features",
                        Scopes = new string[] { Scopes.Read, Scopes.Write }
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
                builder.AddConfigurationStore(idServerOptions =>
                {
                    idServerOptions.ConfigureDbContext = builder =>
                        builder.UseSqlServer(options.SqlServerConnectionString,
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
