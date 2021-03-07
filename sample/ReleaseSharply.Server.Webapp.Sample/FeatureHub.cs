using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ReleaseSharply.Server.Webapp.Sample
{
    public class FeatureHub : Hub
    {
        private readonly ConcurrentDictionary<string, string> _connectionGroupMap = new ConcurrentDictionary<string, string>();

        public async Task AddToGroup(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            _connectionGroupMap.AddOrUpdate(Context.ConnectionId, group, (key, value) => group);
        }

        public async Task SendUpdateAsync(string featureGroup, Feature[] features)
        {
            await Clients?.Group(featureGroup).SendAsync("ReceiveUpdate", features);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_connectionGroupMap.ContainsKey(Context.ConnectionId))
            {
                var group = _connectionGroupMap[Context.ConnectionId];
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

    public class Feature
    {
        public Feature()
        {

        }

        public Feature(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

}