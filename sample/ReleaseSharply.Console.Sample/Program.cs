using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReleaseSharply.Client;
using System;
using System.Threading.Tasks;

namespace ReleaseSharply.Console.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            await Task.Delay(TimeSpan.FromSeconds(5));

            var clientOptions = new ReleaseSharplyClientOptions
            {
                ServerHostname = "https://localhost:5001",
                FeatureGroup = "ConsoleFeatures",
                Username = "ConsoleClient",
                Password = "SuperSecretPassword"
            };

            var logger = services.BuildServiceProvider().GetRequiredService<ILogger<FeatureManager>>();

            var manager = new FeatureManager(clientOptions, logger);
            await manager.StartAsync();

            await Task.Delay(TimeSpan.FromMinutes(100));
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
        }
    }
}
