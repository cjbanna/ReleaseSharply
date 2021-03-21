using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ReleaseSharply.Server.Data;
using ReleaseSharply.Server.Models;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ReleaseSharply.Server
{
    public static class ApplicationBuildExtensions
    {
        public static IApplicationBuilder UseReleaseSharply(this IApplicationBuilder app)
        {
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapHub<FeatureHub>("/featurehub")
                    .RequireAuthorization(nameof(Policies.FeatureFlagRead));

                var featureHub = endpoints.ServiceProvider.GetService<FeatureHub>();
                //var featureManager = endpoints.ServiceProvider.GetService<IFeatureManager>();

                // Features
                endpoints.MapGet("api/features", async context =>
                {
                    using (var scope = endpoints.ServiceProvider.CreateScope())
                    {
                        var featureManager = scope.ServiceProvider.GetService<IFeatureManager>();
                        var featureGroup = context.Request.Query["featureGroup"].ToString();
                        var features = await featureManager.GetFeaturesAsync(featureGroup);

                        var body = JsonSerializer.Serialize(features);
                        var bytes = Encoding.UTF8.GetBytes(body);
                        context.Response.ContentType = "application/json";
                        await context.Response.BodyWriter.WriteAsync(bytes);
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagRead));

                // FeatureGroup Subscribe
                endpoints.MapPost("api/featureGroups/{featureGroup}/subscribe", async context =>
                {
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        var featureGroup = context.Request.RouteValues["featureGroup"]?.ToString();
                        if (string.IsNullOrWhiteSpace(featureGroup))
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        }
                        else
                        {
                            await featureHub.AddToGroup(featureGroup);
                            context.Response.StatusCode = StatusCodes.Status200OK;
                        }
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagRead));

                // Feature Add
                endpoints.MapPost("api/features", async context =>
                {
                    using (var scope = endpoints.ServiceProvider.CreateScope())
                    {
                        var featureManager = scope.ServiceProvider.GetService<IFeatureManager>();

                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            var body = await reader.ReadToEndAsync();
                            var feature = JsonSerializer.Deserialize<Feature>(body);

                            await featureManager.AddFeatureAsync(feature);

                            context.Response.StatusCode = StatusCodes.Status200OK;
                        }
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagWrite));

                // Feature Update
                endpoints.MapPut("api/features", async context =>
                {
                    using (var scope = endpoints.ServiceProvider.CreateScope())
                    {
                        var featureManager = scope.ServiceProvider.GetService<IFeatureManager>();

                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            var body = await reader.ReadToEndAsync();
                            var feature = JsonSerializer.Deserialize<Feature>(body);

                            await featureManager.UpdateFeatureAsync(feature);

                            context.Response.StatusCode = StatusCodes.Status200OK;
                        }
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagWrite));

                // FeatureGroup Add
                endpoints.MapPost("api/featureGroups", async context =>
                {
                    using (var scope = endpoints.ServiceProvider.CreateScope())
                    {
                        var featureManager = scope.ServiceProvider.GetService<IFeatureManager>();

                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            var body = await reader.ReadToEndAsync();
                            var featureGroup = JsonSerializer.Deserialize<FeatureGroup>(body);

                            await featureManager.AddFeatureGroupAsync(featureGroup);

                            context.Response.StatusCode = StatusCodes.Status200OK;
                        }
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagWrite));
            });

            return app;
        }
    }
}
