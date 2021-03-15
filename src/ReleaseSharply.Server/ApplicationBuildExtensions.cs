using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ReleaseSharply.Server
{
    public static class ApplicationBuildExtensions
    {
        public static IApplicationBuilder UseReleaseSharply(this IApplicationBuilder app, Action<ReleaseSharplyOptions> configure = null)
        {
            var options = new ReleaseSharplyOptions();
            configure(options);

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints
                    .MapHub<FeatureHub>("/featurehub")
                    .RequireAuthorization(nameof(Policies.FeatureFlagRead));

                var featureHub = endpoints.ServiceProvider.GetService<FeatureHub>();

                endpoints.MapGet("api/featureGroups/{featureGroup}", async context =>
                {
                    var featureGroup = context.Request.RouteValues["featureGroup"];
                    var features = options.FeatureProvider?.Invoke(featureGroup?.ToString());

                    var body = JsonSerializer.Serialize(features);
                    var bytes = Encoding.UTF8.GetBytes(body);
                    await context.Response.BodyWriter.WriteAsync(bytes);
                }).RequireAuthorization(nameof(Policies.FeatureFlagRead));

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

                endpoints.MapPost("api/features/publish", async context =>
                {
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        var body = await reader.ReadToEndAsync();
                        var request = JsonSerializer.Deserialize<PublishRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        await featureHub.SendUpdateAsync(request.FeatureGroup, new[] { new Feature(request.Name, request.IsEnabled) });

                        context.Response.StatusCode = StatusCodes.Status200OK;
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagWrite)); ;

                endpoints.MapPost("foo", async context =>
                {
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        var body = await reader.ReadToEndAsync();

                        var bytes = Encoding.UTF8.GetBytes(body);
                        await context.Response.BodyWriter.WriteAsync(bytes);
                    }
                }).RequireAuthorization(nameof(Policies.FeatureFlagWrite));
            });

            return app;
        }
    }
}
