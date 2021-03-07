using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReleaseSharply.Client
{
    public interface IFeatureManager
    {
        Task<bool> IsEnabledAsync(string feature);
    }

    public class FeatureManager : IFeatureManager
    {
        private readonly ConcurrentDictionary<string, bool> _features;

        public FeatureManager()
        {
            _features = new ConcurrentDictionary<string, bool>();
        }

        public async Task<bool> IsEnabledAsync(string feature)
        {
            return await Task.FromResult(_features.GetValueOrDefault(feature, false));
        }

        public async Task StartAsync()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/featurehub", HttpTransportType.ServerSentEvents)
                .WithAutomaticReconnect()
                .Build();

            connection.On<Feature[]>("ReceiveUpdate", OnReceiveUpdate);

            await connection.StartAsync();
        }

        private void OnReceiveUpdate(Feature[] features)
        {
            Console.WriteLine("---");
            foreach (var feature in features)
            {
                //Console.WriteLine($"{feature.Name}:{feature.IsEnabled}");
                _features.AddOrUpdate(feature.Name, feature.IsEnabled, (name, _) => feature.IsEnabled);
            }

            foreach (var feature in _features)
            {
                Console.WriteLine($"{feature.Key}:{feature.Value}");
            }
        }
    }

    public class Feature
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

}
