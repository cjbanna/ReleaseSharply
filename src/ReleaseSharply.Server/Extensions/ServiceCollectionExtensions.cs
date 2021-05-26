using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReleaseSharply.Server.Data;
using ReleaseSharply.Server.Options;
using System;

namespace ReleaseSharply.Server.Extensions
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
                .AddTransient<IFeatureManager, FeatureManager>()
                .AddFeaturesDbContext(options);

            var isDevelopment = false;
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var env = scope.ServiceProvider.GetService<IWebHostEnvironment>();
                isDevelopment = env.IsDevelopment();
            }

            services
                .AddIdentityServer()
                .AddReleaseSharplyPersistence(options)
                .AddReleaseSharplyCredentials(options, isDevelopment);

            services.AddReleaseSharplyAuth(options, isDevelopment);

            return services;
        }

        internal static IServiceCollection AddFeaturesDbContext(this IServiceCollection services, ReleaseSharplyOptions options)
        {
            return services.AddDbContext<FeaturesDbContext>(dbContextOptions =>
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
        }

        internal static IServiceCollection AddReleaseSharplyAuth(this IServiceCollection services, ReleaseSharplyOptions options, bool isDevelopment)
        {
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
