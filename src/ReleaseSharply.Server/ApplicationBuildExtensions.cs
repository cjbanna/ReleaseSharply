using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ReleaseSharply.Server.Models;
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
                        }
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagRead));

                // Features
                endpoints.MapGet("api/features", async context =>
                {
                    using (var scope = endpoints.ServiceProvider.CreateScope())
                    {
                        var featureManager = scope.ServiceProvider.GetService<IFeatureManager>();
                        var featureGroup = context.Request.Query["featureGroup"].ToString();

                        var features = await featureManager.GetFeaturesAsync(featureGroup);

                        context.Response.ContentType = "application/json";
                        await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(features)));
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

                            var persistedFeature = await featureManager.AddFeatureAsync(feature);

                            context.Response.ContentType = "application/json";
                            await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(persistedFeature)));
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

                            var persistedFeature = await featureManager.UpdateFeatureAsync(feature);

                            context.Response.ContentType = "application/json";
                            await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(persistedFeature)));
                        }
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagWrite));

                // Feature Delete
                endpoints.MapDelete("api/features", async context =>
                {
                    using (var scope = endpoints.ServiceProvider.CreateScope())
                    {
                        var featureManager = scope.ServiceProvider.GetService<IFeatureManager>();

                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            var body = await reader.ReadToEndAsync();
                            var feature = JsonSerializer.Deserialize<Feature>(body);

                            await featureManager.RemoveFeatureAsync(feature);
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

                            var persistedFeatureGroup = await featureManager.AddFeatureGroupAsync(featureGroup);

                            context.Response.ContentType = "application/json";
                            await context.Response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(persistedFeatureGroup)));
                        }
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagWrite));
            });

            return app;
        }
    }
}
