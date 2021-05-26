using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReleaseSharply.Server.Options;
using System;
using System.Reflection;

namespace ReleaseSharply.Server.Extensions
{
    internal static class IdentityServerBuilderExtensions
    {
        internal static IIdentityServerBuilder AddReleaseSharplyPersistence(this IIdentityServerBuilder builder, ReleaseSharplyOptions options)
        {
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

            return builder;
        }

        internal static IIdentityServerBuilder AddReleaseSharplyCredentials(this IIdentityServerBuilder builder, ReleaseSharplyOptions options, bool isDevelopment)
        {
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

            return builder;
        }

        
    }
}
