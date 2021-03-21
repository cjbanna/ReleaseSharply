using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ReleaseSharply.Server.Models
{
    public class FeatureGroup
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonIgnore]
        public IList<Feature> Features { get; set; }
    }
}
