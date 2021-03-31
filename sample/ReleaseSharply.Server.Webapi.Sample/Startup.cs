using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ReleaseSharply.Server.Options;
using ReleaseSharply.Server.Webapi.Sample.Options;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
            services.AddControllers();

            services.AddReleaseSharply(options =>
            {
                options.Clients = new[]
                {
                    // TODO: make this into a builder? Use Azure Key Vault as example?
                    // .AddClient(string clientId, string secret).WithReadScope()
                    // .AddClient().WithWriteScope()

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

                // Setup localhost SSL cert
                var appOptions = new ConsoleAppOptions();
                Configuration.Bind(nameof(ConsoleAppOptions), appOptions);

                if (!string.IsNullOrWhiteSpace(appOptions.CertificateBase64))
                {
                    var bytes = Convert.FromBase64String(appOptions.CertificateBase64);
                    var certificate = new X509Certificate2(bytes, appOptions.CertificatePassword);
                    options.SigningCredentials = new X509SigningCredentials(certificate);
                }
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

            app.UseReleaseSharply();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
