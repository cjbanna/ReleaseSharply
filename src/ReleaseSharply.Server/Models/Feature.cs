using System.Text.Json.Serialization;

namespace ReleaseSharply.Server.Models
{
    public class Feature
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("featureGroupId")]
        public int FeatureGroupId { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }
    }
}
