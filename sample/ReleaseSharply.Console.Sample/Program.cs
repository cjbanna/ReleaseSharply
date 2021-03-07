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
            var manager = new FeatureManager();
            await manager.StartAsync();

            await Task.Delay(TimeSpan.FromMinutes(100));
        }
    }
}
