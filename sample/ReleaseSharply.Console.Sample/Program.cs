using Microsoft.AspNetCore.SignalR.Client;
using ReleaseSharply.Client;
using System;
using System.Threading.Tasks;

namespace ReleaseSharply.Console.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var url = "https://localhost:5001";
            var featureGroup = "ConsoleGroup";
            var username = "ConsoleClient";
            var password = "SuperSecretPassword";
            var scope = "features.read";
            var manager = new FeatureManager(url, featureGroup, username, password, scope);
            await manager.StartAsync();

            await Task.Delay(TimeSpan.FromMinutes(100));
        }
    }
}
