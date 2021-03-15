using System.Text.Json.Serialization;

namespace ReleaseSharply.Client
{
    internal class AuthToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; set; }
    }
}
