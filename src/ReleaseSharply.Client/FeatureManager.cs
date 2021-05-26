using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
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
        private ILogger<FeatureManager> _logger;
        private ReleaseSharplyClientOptions _clientOptions;

        public FeatureManager(
            ReleaseSharplyClientOptions clientOptions,
            ILogger<FeatureManager> logger)
        {
            _features = ImmutableDictionary<string, Feature>.Empty;
            _clientOptions = clientOptions;
            _logger = logger;
        }

        public async Task<bool> IsEnabledAsync(string featureName)
        {
            _features.TryGetValue(featureName, out Feature feature);
            var isEnabled = feature?.IsEnabled == true;
            return await Task.FromResult(isEnabled);
        }

        public async Task StartAsync()
        {
            var url = $"{_clientOptions.ServerHostname}/featurehub";
            var connection = new HubConnectionBuilder()
                .WithUrl(url, HttpTransportType.ServerSentEvents, options =>
                {
                    options.AccessTokenProvider = () => GetTokenAsync();
                })
                .WithAutomaticReconnect()
                .Build();

            connection.On<Feature[]>("ReceiveUpdate", OnReceiveUpdate);
            connection.On<Feature>("OnRemoved", OnRemoved);
            connection.Closed += OnClosedAsync;
            connection.Reconnecting += OnReconnectingAsync;
            connection.Reconnected += OnReconnectedAsync;

            _logger.LogInformation($"Connecting to {url}");

            try
            {
                await connection.StartAsync();
                _logger.LogInformation($"Connected to ReleaseSharply at {url}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to ReleaseSharply server");
                throw;
            }

            _features = await RefreshFeaturesAsync();
            _logger.LogInformation(string.Join(',', _features.Select(f => $"{f.Key}:{f.Value.IsEnabled}")));

            await SubscribeToFeatureGroupAsync();
        }

        private async Task<string> GetTokenAsync()
        {
            if (string.IsNullOrEmpty(_authToken) || DateTime.Now >= _tokenExpiration)
            {
                var dict = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "scope", Scopes.Read },
                    { "client_id", _clientOptions.Username },
                    { "client_secret", _clientOptions.Password }
                };

                var client = new HttpClient();
                var response = await client.PostAsync($"{_clientOptions.ServerHostname}/connect/token", new FormUrlEncodedContent(dict));
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

            var url = $"{_clientOptions.ServerHostname}/api/features?featureGroup={_clientOptions.FeatureGroup}";
            _logger.LogInformation($"Refresh features {url}");

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();

            var features = JsonSerializer.Deserialize<Feature[]>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var newFeatures = ImmutableDictionary<string, Feature>.Empty;
            foreach (var feature in features)
            {
                newFeatures = newFeatures.Add(feature.Name, feature);
            }
            return newFeatures;
        }

        private async Task SubscribeToFeatureGroupAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await client.PostAsync($"{_clientOptions.ServerHostname}/api/featureGroups/{_clientOptions.FeatureGroup}/subscribe", null);
            response.EnsureSuccessStatusCode();
        }

        private async Task OnReconnectedAsync(string arg)
        {
            _logger.LogInformation("Reconnected");

            await RefreshFeaturesAsync();
            await SubscribeToFeatureGroupAsync();
        }

        private async Task OnReconnectingAsync(Exception arg)
        {
            _logger.LogInformation("Reconnecting");
            await Task.CompletedTask;
        }

        private async Task OnClosedAsync(Exception arg)
        {
            _logger.LogInformation("Connection closed");
            await Task.CompletedTask;
        }

        private void OnReceiveUpdate(Feature[] features)
        {
            _logger.LogInformation("OnReceiveUpdate");

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

            _logger.LogInformation(string.Join(',', _features.Select(f => $"{f.Key}:{f.Value.IsEnabled}")));
        }

        private void OnRemoved(Feature feature)
        {
            _logger.LogInformation("OnRemoved");

            var newFeatures = _features;
            var idExists = newFeatures.Values.SingleOrDefault(f => f.Id == feature.Id);
            if (idExists != null)
            {
                newFeatures = newFeatures.Remove(idExists.Name);
            }

            _features = newFeatures;

            _logger.LogInformation(string.Join(',', _features.Select(f => $"{f.Key}:{f.Value.IsEnabled}")));
        }
    }
}
