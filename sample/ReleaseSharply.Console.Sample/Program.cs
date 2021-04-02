using ReleaseSharply.Client;
using System;
using System.Threading.Tasks;

namespace ReleaseSharply.Console.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var url = "https://localhost:5001";
            var featureGroup = "ConsoleFeatures";
            var username = "ConsoleClient";
            var password = "SuperSecretPassword";
            var scope = Scopes.Read;
            var manager = new FeatureManager(url, featureGroup, username, password, scope);
            await manager.StartAsync();

            await Task.Delay(TimeSpan.FromMinutes(100));
        }
    }
}
