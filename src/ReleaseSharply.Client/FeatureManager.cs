using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReleaseSharply.Client
{
    public interface IFeatureManager
    {
        Task<bool> IsEnabledAsync(string feature);
    }

    public class FeatureManager : IFeatureManager
    {
        private string _authToken;
        private DateTime _tokenExpiration = DateTime.MinValue;
        private ImmutableDictionary<string, Feature> _features;
        private readonly string _serverHostName;
        private readonly string _featureGroup;
        private readonly string _username;
        private readonly string _password;
        private readonly string _scope;

        public FeatureManager(
            string serverHostName,
            string featureGroup,
            string username,
            string password,
            string scope)
        {
            _features = ImmutableDictionary<string, Feature>.Empty;
            _serverHostName = serverHostName;
            _featureGroup = featureGroup;
            _username = username;
            _password = password;
            _scope = scope;
        }

        public async Task<bool> IsEnabledAsync(string featureName)
        {
            var feature = default(Feature);
            _features.TryGetValue(featureName, out feature);
            var isEnabled = feature?.IsEnabled == true;
            return await Task.FromResult(isEnabled);
        }

        public async Task StartAsync()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{_serverHostName}/featurehub", HttpTransportType.ServerSentEvents, options =>
                {
                    options.AccessTokenProvider = () => GetTokenAsync();
                })
                .WithAutomaticReconnect()
                .Build();

            connection.On<Feature[]>("ReceiveUpdate", OnReceiveUpdate);
            connection.On<Feature>("OnRemoved", OnRemoved);
            connection.Closed += Connection_Closed;
            connection.Reconnecting += Connection_Reconnecting;
            connection.Reconnected += Connection_Reconnected;

            await connection.StartAsync();
            _features = await RefreshFeaturesAsync();
            await SubscribeToFeatureGroup();
        }

        private async Task<string> GetTokenAsync()
        {
            if (string.IsNullOrEmpty(_authToken) || DateTime.Now >= _tokenExpiration)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("grant_type", "client_credentials");
                dict.Add("scope", _scope);
                dict.Add("client_id", _username);
                dict.Add("client_secret", _password);

                var client = new HttpClient();
                var response = await client.PostAsync($"{_serverHostName}/connect/token", new FormUrlEncodedContent(dict));
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var token = JsonSerializer.Deserialize<AuthToken>(body);
                _tokenExpiration = DateTime.Now.AddSeconds(token.ExpiresInSeconds);
                _authToken = token.AccessToken;
            }

            return _authToken;
        }

        private async Task<ImmutableDictionary<string, Feature>> RefreshFeaturesAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var url = $"{_serverHostName}/api/features?featureGroup={_featureGroup}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var features = JsonSerializer.Deserialize<Feature[]>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine(body);

            var newFeatures = ImmutableDictionary<string, Feature>.Empty;
            foreach (var feature in features)
            {
                newFeatures = newFeatures.Add(feature.Name, feature);
            }
            return newFeatures;
        }

        private async Task SubscribeToFeatureGroup()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await client.PostAsync($"{_serverHostName}/api/featureGroups/{_featureGroup}/subscribe", null);
            response.EnsureSuccessStatusCode();
        }

        private async Task Connection_Reconnected(string arg)
        {
            await RefreshFeaturesAsync();
            await SubscribeToFeatureGroup();
        }

        private async Task Connection_Reconnecting(Exception arg)
        {
            Console.WriteLine("Reconnecting");
            return;
        }

        private Task Connection_Closed(Exception arg)
        {
            throw new NotImplementedException();
        }

        private void OnReceiveUpdate(Feature[] features)
        {
            Console.WriteLine("--- Updated ---");

            var newFeatures = _features;
            foreach (var feature in features)
            {
                var idExists = newFeatures.Values.SingleOrDefault(f => f.Id == feature.Id);
                if (idExists != null)
                {
                    newFeatures = newFeatures.Remove(idExists.Name);
                    newFeatures = newFeatures.SetItem(feature.Name, feature);
                }
                else
                {
                    newFeatures = newFeatures.Add(feature.Name, feature);
                }
            }

            _features = newFeatures;

            foreach (var feature in _features)
            {
                Console.WriteLine($"{feature.Key}:{feature.Value.IsEnabled}");
            }
        }

        private void OnRemoved(Feature feature)
        {
            Console.WriteLine("--- Removed ---");

            var newFeatures = _features;
            var idExists = newFeatures.Values.SingleOrDefault(f => f.Id == feature.Id);
            if (idExists != null)
            {
                newFeatures = newFeatures.Remove(idExists.Name);
            }

            _features = newFeatures;

            foreach (var f in _features)
            {
                Console.WriteLine($"{f.Key}:{f.Value.IsEnabled}");
            }
        }
    }
}
