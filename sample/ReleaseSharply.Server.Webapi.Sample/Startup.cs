using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ReleaseSharply.Server.Extensions;
using ReleaseSharply.Server.Options;
using ReleaseSharply.Server.Webapi.Sample.Data;
using ReleaseSharply.Server.Webapi.Sample.Options;
using System;
using System.Security.Cryptography.X509Certificates;

namespace ReleaseSharply.Server.Webapi.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<FeaturesDbSeedDataGenerator>();

            services.AddControllers();

            services.AddReleaseSharply(options =>
            {
                options
                    .AddInMemoryClient(client => client
                        .WithClientId("ConsoleClient")
                        .WithSecret("SuperSecretPassword")
                        .WithReadScope());

                options
                    .AddInMemoryClient(client => client
                        .WithClientId("pub")
                        .WithSecret("SuperSecretPassword")
                        .WithWriteScope());

                // Setup localhost SSL cert
                //var appOptions = new SampleAppOptions();
                //Configuration.Bind(nameof(SampleAppOptions), appOptions);

                //if (!string.IsNullOrWhiteSpace(appOptions.CertificateBase64))
                //{
                //    var bytes = Convert.FromBase64String(appOptions.CertificateBase64);
                //    var certificate = new X509Certificate2(bytes, appOptions.CertificatePassword);
                //    options.SigningCredentials = new X509SigningCredentials(certificate);
                //}

                // TODO: 
                // 1. Authority using different domain

                //options.SqlServerConnectionString = Configuration.GetConnectionString("ReleaseSharplyConnectionString");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetService<FeaturesDbSeedDataGenerator>();
                seeder.SeedData();
            }

            app.UseReleaseSharply();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
