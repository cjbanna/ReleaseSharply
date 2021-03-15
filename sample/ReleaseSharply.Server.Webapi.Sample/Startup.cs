using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace ReleaseSharply.Server.Webapi.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSignalR();
            services.AddControllers();

            //services.AddSingleton<FeatureHub>();
            services.AddReleaseSharply(options =>
            {
                options.ApiResources = new[]
                {
                    new ApiResource
                    {
                        Name = "FeatureFlags",
                        Scopes = new string[] { "features.read", "features.write" },
                        ApiSecrets = new Secret[] { new Secret("ScopeSecret".Sha256()) }
                    }
                };

                options.ApiScopes = new[]
                {
                    new ApiScope("features.read", ""),
                    new ApiScope("features.write", "")
                };

                options.Clients = new[]
                {
                    new Client
                    {
                        ClientId = "ConsoleClient",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientSecrets = new[] { new Secret("SuperSecretPassword".Sha256()) },
                        AllowedScopes = new[] { "features.read" }
                    },
                    new Client
                    {
                        ClientId = "pub",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        ClientSecrets = new[] { new Secret("SuperSecretPassword".Sha256()) },
                        AllowedScopes = new[] { "features.write" }
                    }
                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseReleaseSharply(options =>
            {
                options.FeatureProvider = (featureGroup) =>
                {
                    switch (featureGroup)
                    {
                        case "ConsoleGroup":
                            return new[]
                            {
                                new Feature("foo", false),
                                new Feature("bar", true)
                            };
                        default:
                            return Enumerable.Empty<Feature>();
                    }
                };
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
