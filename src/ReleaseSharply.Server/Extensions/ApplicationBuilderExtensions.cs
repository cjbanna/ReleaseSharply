using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReleaseSharply.Server.Data;
using System.Collections.Generic;
using System.Linq;
using Entities = IdentityServer4.EntityFramework.Entities;

namespace ReleaseSharply.Server.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void InitializeDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var featuresDbContext = serviceScope.ServiceProvider.GetRequiredService<FeaturesDbContext>();
                featuresDbContext.Database.EnsureCreated();

                var context = serviceScope.ServiceProvider.GetService<ConfigurationDbContext>();
                if (context == null)
                {
                    // just bail if no context is setup
                    return;
                }

                context.Database.Migrate();
                
                if (!context.Clients.Any())
                {
                    var clients = new List<Entities.Client>
                    {
                        new Entities.Client
                        {
                            AllowedGrantTypes = GrantTypes.ClientCredentials
                                .Select(c => new Entities.ClientGrantType { GrantType = c })
                                .ToList(),
                            ClientId = "ConsoleClient",
                            ClientSecrets = new List<Entities.ClientSecret> { new Entities.ClientSecret { Value = "SuperSecretPassword".Sha256() } },
                            AllowedScopes = new List<Entities.ClientScope> { new Entities.ClientScope {  Scope = Scopes.Read} }
                        },
                        new Entities.Client
                        {
                            AllowedGrantTypes = GrantTypes.ClientCredentials
                                .Select(c => new Entities.ClientGrantType { GrantType = c })
                                .ToList(),
                            ClientId = "pub",
                            ClientSecrets = new List<Entities.ClientSecret> { new Entities.ClientSecret { Value = "SuperSecretPassword".Sha256() } },
                            AllowedScopes = new List<Entities.ClientScope> { new Entities.ClientScope {  Scope = Scopes.Write} }
                        }
                    };
                    context.Clients.AddRange(clients);
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    var resource = new Entities.ApiResource
                    {
                        Name = "Features",
                        Scopes = new List<Entities.ApiResourceScope>
                        {
                            new Entities.ApiResourceScope { Scope = Scopes.Read },
                            new Entities.ApiResourceScope { Scope = Scopes.Write }
                        }
                    };
                    context.ApiResources.Add(resource);
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    var scopes = new[]
                    {
                        new Entities.ApiScope { Name = Scopes.Read },
                        new Entities.ApiScope { Name = Scopes.Write }
                    };
                    context.ApiScopes.AddRange(scopes);
                    context.SaveChanges();
                }
            }
        }
    }
}
